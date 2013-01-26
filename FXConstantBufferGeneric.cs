using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

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

    public class FXConstantBuffer<T> : IDisposable, IFXResources
        where T : struct
    {
        private readonly Buffer buffer;
        private readonly DataStream dataStream;

        private readonly Boolean is_exist;
        internal ShaderType whereIsExist = ShaderType.None;
        private Boolean isDirty;
        private T localValue;

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



        #region Properties

        #region Slots

        /// <summary>
        /// Get the slot of the buffer in pixel shader 
        /// </summary>
        public int Slot_PS { get { return slot_ps; } }

        /// <summary>
        /// Get the slot of the buffer in vertex shader 
        /// </summary>
        public int Slot_VS { get { return slot_vs; } }

        /// <summary>
        /// Get the slot of the buffer in compute shader 
        /// </summary>
        public int Slot_CS { get { return slot_cs; } }

        /// <summary>
        /// Get the slot of the buffer in geometry shader 
        /// </summary>
        public int Slot_GS { get { return slot_gs; } }

        #endregion


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


        #endregion



        #region Constructor

        /// <summary>
        /// Create a new constant buffer 
        /// </summary>
        /// <param name="resource_name"></param>
        /// <param name="shaderReflection"></param>
        public FXConstantBuffer( Device dev, string resource_name, 
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

        }

        #endregion


        /// <summary>
        /// Update the value and send the new values to the hardware
        /// </summary>
        /// <param name="value"></param>
        public void UpdateValue( T value )
        {
            if ( isExist ) {

                // check if we have actual change with our variable
                if ( !value.Equals(localValue))
                {
                    // set the local value
                    localValue = value;

                    // set that we need to commit the changes to the hardware
                    isDirty = true;
                }
            }

        }


        /// <summary>
        /// Commit any memmory changes and set the buffers
        /// </summary>
        /// <param name="deviceContext"></param>
        /// <param name="type"></param>
        public void Commit( DeviceContext deviceContext, ShaderType type )
        {
            if ( isExist ) {
                // commit the memmory changes
                if ( isDirty ) {
                    // If no specific marshalling is needed, can use 
                    // dataStream.Write(value) for better performance.
                    //Marshal.StructureToPtr( value, dataStream.DataPointer, false );

                    dataStream.Seek( 0, System.IO.SeekOrigin.Begin );
                    dataStream.Write<T>( localValue );
                    dataStream.Seek( 0, System.IO.SeekOrigin.Begin );

                    // pass the data to the gpu
                    var dataBox = new DataBox(dataStream.DataPointer, 0, 0);
                    deviceContext.UpdateSubresource( dataBox, buffer, 0 );

                    isDirty = false;
                }

                // set the buffer to the shader
                switch ( type ) {
                    case ShaderType.Pixel:
                        deviceContext.PixelShader.SetConstantBuffer( Slot_PS, Buffer );
                        break;
                    case ShaderType.Vertex:
                        deviceContext.VertexShader.SetConstantBuffer(Slot_VS, Buffer );
                        break;
                    case ShaderType.Compute:
                        deviceContext.ComputeShader.SetConstantBuffer(Slot_CS, Buffer );
                        break;
                    case ShaderType.Geometry:
                        deviceContext.GeometryShader.SetConstantBuffer(Slot_GS, Buffer );
                        break;
                }
            }
        }

        /// <summary>
        /// Clean the resource binding
        /// </summary>
        /// <param name="deviceContext"></param>
        /// <param name="type"></param>
        public void CleanBind(DeviceContext deviceContext, ShaderType type)
        {
            
        }

        public void Dispose()
        {
            if ( dataStream != null )
                dataStream.Dispose();
            if ( buffer != null )
                buffer.Dispose();
        }
    }
}
