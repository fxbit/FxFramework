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


namespace FXFramework
{
    public class FXConstantBuffer :  IDisposable, IFXResources
    {
        internal readonly Buffer buffer;
        internal readonly DataStream dataStream;

        #region Shader buffer bindDesc slots

        internal readonly ConstantBuffer constantBuffer_ps;
        internal readonly InputBindingDescription inputBindingDesc_ps;
        internal readonly int slot_ps;

        internal readonly ConstantBuffer constantBuffer_vs;
        internal readonly InputBindingDescription inputBindingDesc_vs;
        internal readonly int slot_vs;

        internal readonly ConstantBuffer constantBuffer_cs;
        internal readonly InputBindingDescription inputBindingDesc_cs;
        internal readonly int slot_cs;

        internal readonly ConstantBuffer constantBuffer_gs;
        internal readonly InputBindingDescription inputBindingDesc_gs;
        internal readonly int slot_gs;

        #endregion

        internal Boolean isDirty;

        internal readonly Boolean is_exist;
        internal ShaderType whereIsExist = ShaderType.None;

        internal List<IFXVariable> ListWithVariables;
        internal Boolean IsDisposed = false;

        /// <summary>
        /// This field is NOT valid.
        /// </summary>
        public int Slot { get { return 0; } }


        /// <summary>
        /// Is CB exist ?
        /// </summary>
        public Boolean isExist
        {
            get { return is_exist; }
        }

        /// <summary>
        /// Get the internal buffer
        /// </summary>
        public Buffer Buffer
        {
            get { return buffer; }
        }

        /// <summary>
        /// Get the bit field for the shaders that this buffer exist
        /// </summary>
        public ShaderType WhereIsExist
        {
            get { return whereIsExist; }
        }

        #region Slots

        /// <summary>
        /// Get the slot of the buffer in pixel shader 
        /// </summary>
        public int PS_Slot { get { return slot_ps; } }

        /// <summary>
        /// Get the slot of the buffer in vertex shader 
        /// </summary>
        public int VS_Slot { get { return slot_vs; } }

        /// <summary>
        /// Get the slot of the buffer in compute shader 
        /// </summary>
        public int CS_Slot { get { return slot_cs; } }

        /// <summary>
        /// Get the slot of the buffer in geometry shader 
        /// </summary>
        public int GS_Slot { get { return slot_gs; } }

        #endregion


        #region Constructor

        public FXConstantBuffer( Device dev, String resource_name,
            ShaderReflection psShaderReflection = null, ShaderReflection vsShaderReflection = null,
            ShaderReflection csShaderReflection = null, ShaderReflection gsShaderReflection = null )
        {
            int size=0;

            /// ----------------- Get the Pixel shader buffer ------------------------
            /// 
            #region Pixel Shader
            try {
                if ( psShaderReflection != null ) {
                    // get the constant buffer with the specific name
                    constantBuffer_ps = psShaderReflection.GetConstantBuffer( resource_name );

                    inputBindingDesc_ps = psShaderReflection.GetResourceBindingDescription( resource_name );

                    // find the bind point of the constant buffer
                    slot_ps = inputBindingDesc_ps.BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Pixel;

                    // set the size
                    size = constantBuffer_ps.Description.Size;
                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

            /// ----------------- Get the Vertex shader buffer ------------------------
            /// 
            #region Vertex Shader
            try {
                if ( vsShaderReflection != null ) {
                    // get the constant buffer with the specific name
                    constantBuffer_vs = vsShaderReflection.GetConstantBuffer( resource_name );

                    inputBindingDesc_vs = vsShaderReflection.GetResourceBindingDescription( resource_name );

                    // find the bind point of the constant buffer
                    slot_vs = inputBindingDesc_vs.BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Vertex;

                    // set the size
                    size = constantBuffer_vs.Description.Size;
                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

            /// ----------------- Get the Compute shader buffer ------------------------
            /// 
            #region Compute Shader

            try {
                if ( csShaderReflection != null ) {
                    // get the constant buffer with the specific name
                    constantBuffer_cs = csShaderReflection.GetConstantBuffer( resource_name );

                    inputBindingDesc_cs = csShaderReflection.GetResourceBindingDescription( resource_name );

                    // find the bind point of the constant buffer
                    slot_cs = inputBindingDesc_cs.BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Compute;

                    // set the size
                    size = constantBuffer_cs.Description.Size;
                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

            /// ----------------- Get the Geometry shader buffer ------------------------

            #region Geometry Shader

            try {
                if ( gsShaderReflection != null ) {
                    // get the constant buffer with the specific name
                    constantBuffer_gs = gsShaderReflection.GetConstantBuffer( resource_name );

                    inputBindingDesc_gs = gsShaderReflection.GetResourceBindingDescription( resource_name );

                    // find the bind point of the constant buffer
                    slot_gs = inputBindingDesc_gs.BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Geometry;

                    // set the size
                    size = constantBuffer_gs.Description.Size;
                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion


            // check that we have found someware this cb
            if ( whereIsExist == ShaderType.None ) {
                is_exist = false;
                return;
            } else
                is_exist = true;

            #region Create buffer

            // create a new buffer for the constant buffer
            buffer = new Buffer( dev, new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                SizeInBytes = size,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            } );

            // create a data stream for the struct
            dataStream = new DataStream( size, true, true );

            #endregion

            //init the list of variables
            ListWithVariables = new List<IFXVariable>();
        }

        #endregion


        #region GetMemberByName

        public FXVariable<T> GetMemberByName<T>( String ResourceName ) where T : struct
        {
            // check if the cb exist to the shaders
            if ( is_exist ) {
                /// init variable base on the name 
                FXVariable<T> tmpVar = new FXVariable<T>( this, ResourceName );

                /// add the variable to the list
                ListWithVariables.Add( tmpVar );


                return tmpVar;
            }

            return null;
        }

        #endregion


        #region Commit

        public void Commit( DeviceContext deviceContext, ShaderType type )
        {
            // check if we this cb exist in the specific shader type
            if ( !WhereIsExist.HasFlag( type ) )
                return;

            if ( isDirty ) {

                // pass all the variables and update the data stream
                foreach ( IFXVariable var in ListWithVariables ) {
                    if ( var.isDirty ) {

                        // copy the variable value to the data stream
                        var.Commit();
                    }
                }

                // reset the seek 
                dataStream.Seek( 0, System.IO.SeekOrigin.Begin );

                // pass the data to the gpu
                var dataBox = new DataBox(dataStream.DataPointer, 0, 0);
                deviceContext.UpdateSubresource( dataBox, buffer, 0 );

                // clean the flag 
                isDirty = false;
            }

            // we can use switch here beacuse we call this commit only for one shader type
            switch ( type ) {
                case ShaderType.Pixel:
                    deviceContext.PixelShader.SetConstantBuffer(slot_ps, Buffer);
                    break;
                case ShaderType.Vertex:
                    deviceContext.VertexShader.SetConstantBuffer(slot_vs, Buffer);
                    break;
                case ShaderType.Compute:
                    deviceContext.ComputeShader.SetConstantBuffer(slot_cs, Buffer);
                    break;
                case ShaderType.Geometry:
                    deviceContext.GeometryShader.SetConstantBuffer(slot_gs, Buffer);
                    break;
            }
        }

        #endregion


        #region Clean Binding

        /// <summary>
        /// Clean the resource binding
        /// </summary>
        /// <param name="deviceContext"></param>
        /// <param name="type"></param>
        public void CleanBind(DeviceContext deviceContext, ShaderType type)
        {
        }

        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            if ( !IsDisposed ) {
                // clean the buffer
                if ( buffer != null )
                    buffer.Dispose();
                
                // clean the flag of disposed
                IsDisposed = true;
            }
        }

        #endregion
    }
}
