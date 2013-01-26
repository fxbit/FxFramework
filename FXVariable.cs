using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

// resolve conflict - DXGI.Device & Direct3D10.Device
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Effect = SharpDX.Direct3D11.Effect;
using EffectFlags = SharpDX.D3DCompiler.EffectFlags;
using System.Runtime.InteropServices;



namespace FXFramework
{
    // TODO: Check the type that we give with the type that is expect

    public class FXVariable<T> : IFXVariable , IDisposable
        where T : struct
    {
        private FXConstantBuffer CBParent;

        private T localValue;

        private readonly int StartOffset;

        private Boolean is_Dirty;

        /// <summary>
        /// We have changes to the buffer ?
        /// </summary>
        public Boolean isDirty
        {
            get { return is_Dirty; }
        }



        #region Constructor


        public FXVariable( FXConstantBuffer CBParent, String resource_name )
        {
            // save the local variables
            this.CBParent = CBParent;

            ShaderReflectionVariable srv = null;


            // get the offset of the variable from the start of the constant buffer
            if ( CBParent.whereIsExist.HasFlag( ShaderType.Vertex ) ) {

                srv = CBParent.constantBuffer_vs.GetVariable( resource_name );

            } else if ( CBParent.whereIsExist.HasFlag( ShaderType.Pixel ) ) {

                srv = CBParent.constantBuffer_ps.GetVariable( resource_name );

            } else if ( CBParent.whereIsExist.HasFlag( ShaderType.Compute ) ) {

                srv = CBParent.constantBuffer_cs.GetVariable( resource_name );

            } else if ( CBParent.whereIsExist.HasFlag( ShaderType.Geometry ) ) {

                srv =  CBParent.constantBuffer_gs.GetVariable( resource_name );

            }

            // check the type compatibility
            if ( Utils.CheckCompatibilityOfType<T>( localValue, srv.GetVariableType().Description ) ) {
                // get the offset of the variable
                StartOffset = srv.Description.StartOffset;
            } else {
                throw new System.ApplicationException( "Wrong Shader Type" );
            }

            // set the dirty to false because we don't have actual data to send to GPU
            is_Dirty = false;
        }

        #endregion



        #region Set

        public void Set( T value )
        {
            lock ( localValue ) {
                // if the input value is matrix then fix the row-Col issue
                if ( value is Matrix ) {
                    Matrix tmp = value as Matrix? ?? Matrix.Identity; // fucking coding 
                    value = Matrix.Transpose( tmp ) as T? ?? value;
                }

                // check if we have actual change with our variable
                if ( !value.Equals( localValue ) ) {

                    // set the local value
                    localValue = value;

                    // set that we need to commit the changes to the hardware
                    is_Dirty = true;

                    // set the dirtyness and to the parent
                    CBParent.isDirty = true;
                }
            }
        }

        #endregion



        #region Commit

        /// <summary>
        /// Update the data in datastream of the constant buffer if the data are dirty.
        /// </summary>
        /// <returns>If we have change</returns>
        public Boolean Commit()
        {
            // commit the memmory changes
            if ( is_Dirty ) {

                //lock ( localValue ) 
                {
                //    lock ( CBParent.dataStream ) 
                    {
                        // pass the data to the data stream of the constant buffer
                        CBParent.dataStream.Seek( StartOffset, System.IO.SeekOrigin.Begin );
                        CBParent.dataStream.Write<T>( localValue );
                    }
                }

                // reset the dirtyness
                is_Dirty = false;

                // return that we have change
                return true;
            }

            // return that we have no change
            return false;
        }

        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}
