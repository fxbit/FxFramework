using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX.D3DCompiler;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using D3D = SharpDX.Direct3D11;

using FxMaths.GMaps;
using SharpDX.Direct3D;

namespace FXFramework
{
    public class FXResourceVariable : IFXResources,IDisposable
    {
        #region Slots for shaders

        public readonly int Slot_PS;
        public readonly int Slot_VS;
        public readonly int Slot_CS;
        public readonly int Slot_GS;

        #endregion


        public ShaderType whereIsExist = ShaderType.None;
        private ShaderResourceView localSRV;
        private UnorderedAccessView localUAV;
        private AccessViewType accessViewType = AccessViewType.SRV;
        private String resourceName;


        #region Properties

        /// <summary>
        /// Get the bit field for the shaders that this buffer exist
        /// </summary>
        public ShaderType WhereIsExist
        {
            get { return whereIsExist; }
        }

        /// <summary>
        /// The name of the resource(like debug name).
        /// </summary>
        public String ResourceName
        {
            get { return resourceName; }
        }

        #endregion



        #region Constructor

        public FXResourceVariable( String resource_name, 
            ShaderReflection psShaderReflection = null, ShaderReflection vsShaderReflection = null,
            ShaderReflection csShaderReflection = null, ShaderReflection gsShaderReflection = null )
        {
            resourceName = resource_name;

            /// ----------------- Get the Pixel shader buffer ------------------------
            /// 
            #region Pixel Shader
            try {
                if ( psShaderReflection != null ) {

                    InputBindingDescription ibd = psShaderReflection.GetResourceBindingDescription( resource_name );

                    // find the bind point of the Resources
                    Slot_PS = ibd.BindPoint;
                    
                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Pixel;

                    // find the access type
                    if ( ibd.Type == ShaderInputType.UnorderedAccessViewRWByteAddress  || (int)ibd.Type == 4 )
                        accessViewType = AccessViewType.UAV;
                    else
                        accessViewType = AccessViewType.SRV;

                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

            /// ----------------- Get the Vertex shader buffer ------------------------
            /// 
            #region Vertex Shader
            try {
                if ( vsShaderReflection != null ) {

                    // find the bind point of the Resources
                    Slot_VS = vsShaderReflection.GetResourceBindingDescription( resource_name ).BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Vertex;

                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

            /// ----------------- Get the Compute shader buffer ------------------------
            /// 
            #region Compute Shader

            try {
                if ( csShaderReflection != null ) {
                    
                    InputBindingDescription ibd = csShaderReflection.GetResourceBindingDescription( resource_name );
                    
                    // find the bind point of the Resources
                    Slot_CS = ibd.BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Compute;

                    // find the access type
                    if (ibd.Type == ShaderInputType.UnorderedAccessViewRWByteAddress || ibd.Type == ShaderInputType.UnorderedAccessViewRWStructured || ibd.Type == ShaderInputType.UnorderedAccessViewRWStructuredWithCounter || (int)ibd.Type == 4)
                        accessViewType = AccessViewType.UAV;
                    else
                        accessViewType = AccessViewType.SRV;
                    
                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

            /// ----------------- Get the Geometry shader buffer ------------------------

            #region Geometry Shader

            try {
                if ( gsShaderReflection != null ) {

                    // find the bind point of the Resources
                    Slot_GS = gsShaderReflection.GetResourceBindingDescription( resource_name ).BindPoint;

                    // set the bit for the pixel shader
                    whereIsExist |= ShaderType.Geometry;

                }
            } catch ( Exception ex ) { /* do nothing */  }

            #endregion

        }

        #endregion



        #region Set
        /// <summary>
        /// Set the view resource 
        /// </summary>
        /// <param name="srv"></param>
        public void SetResource( ShaderResourceView srv )
        {
            // store the srv local
            localSRV = srv;

            // rest the uav 
            localUAV = null;
        }

        /// <summary>
        /// Set the view resource 
        /// </summary>
        /// <param name="uav"></param>
        public void SetResource( UnorderedAccessView uav )
        {
            // unlink the local srv
            localSRV = null;

            // store the uav local
            localUAV = uav;
        }
        #endregion



        #region Commit

        public void Commit( DeviceContext deviceContext, ShaderType type )
        {
            // check if we this cb exist in the specific shader type
            if ( !WhereIsExist.HasFlag( type ) )
                return;


            // we can use switch here beacuse we call this commit only for one shader type
            switch ( type ) {
                case ShaderType.Pixel:
                    if ( accessViewType == AccessViewType.SRV )
                        deviceContext.PixelShader.SetShaderResource(Slot_PS , localSRV);
                    //else
                        //deviceContext.PixelShader.SetUnorderedAccessView( localUAV, Slot_PS );
                    break;
                case ShaderType.Vertex:
                    deviceContext.VertexShader.SetShaderResource(Slot_VS, localSRV);
                    break;
                case ShaderType.Compute:
                    if ( accessViewType == AccessViewType.SRV)
                        deviceContext.ComputeShader.SetShaderResource(Slot_CS, localSRV);
                    else
                        deviceContext.ComputeShader.SetUnorderedAccessView( Slot_CS, localUAV );
                    break;
                case ShaderType.Geometry:
                    deviceContext.GeometryShader.SetShaderResource(Slot_GS, localSRV);
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
            // check if we this cb exist in the specific shader type
            if (!WhereIsExist.HasFlag(type))
                return;


            // we can use switch here beacuse we call this commit only for one shader type
            switch (type)
            {
                case ShaderType.Pixel:
                    if (accessViewType == AccessViewType.SRV)
                        deviceContext.PixelShader.SetShaderResource(Slot_CS, null);
                    break;
                case ShaderType.Vertex:
                    deviceContext.VertexShader.SetShaderResource(Slot_VS, null);
                    break;
                case ShaderType.Compute:
                    if (accessViewType == AccessViewType.SRV)
                        deviceContext.ComputeShader.SetShaderResource(Slot_CS, null);
                    else
                        deviceContext.ComputeShader.SetUnorderedAccessView(Slot_CS, null);
                    break;
                case ShaderType.Geometry:
                    deviceContext.GeometryShader.SetShaderResource(Slot_GS, null);
                    break;
            }
        }

        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            if (localSRV != null)
                localSRV.Dispose();

            if (localUAV != null)
                localUAV.Dispose();
        }

        #endregion



        #region UAV/Staging

        public static ShaderResourceView InitSRVResource(SharpDX.Direct3D11.Device dev, SharpDX.Direct3D11.Buffer buffer)
        {
            // create desc for the UAV
            ShaderResourceViewDescription srvbufferDesc = new ShaderResourceViewDescription
            {
                Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                Format = Format.Unknown,
                Buffer = new ShaderResourceViewDescription.BufferResource
                {
                    FirstElement = 0,
                    ElementCount = buffer.Description.SizeInBytes / buffer.Description.StructureByteStride,

                },
            };

            ShaderResourceView tmpSRV = new ShaderResourceView(dev, buffer, srvbufferDesc);
            tmpSRV.DebugName = "SRV_" + buffer.DebugName;

            return tmpSRV;

        }


        public static UnorderedAccessView InitUAVResource(SharpDX.Direct3D11.Device dev, SharpDX.Direct3D11.Buffer buffer)
        {
            // create desc for the UAV
            UnorderedAccessViewDescription uavbufferDesc = new UnorderedAccessViewDescription
            {
                Dimension = UnorderedAccessViewDimension.Buffer,
                Format = Format.Unknown,
                Buffer = new UnorderedAccessViewDescription.BufferResource
                {
                    FirstElement = 0,
                    ElementCount = buffer.Description.SizeInBytes / buffer.Description.StructureByteStride,
                },
            };

            
            UnorderedAccessView tmpUAV = new UnorderedAccessView(dev, buffer, uavbufferDesc);
            tmpUAV.DebugName = "UAV_"+buffer.DebugName;

            return tmpUAV;
        }

        /// <summary>
        /// Read the GPU buffer to local array.
        /// The buffer must be a staging buffer ....
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dev"></param>
        /// <param name="cpuBuffer"></param>
        /// <param name="gpuBuffer"></param>
        /// <param name="localBuffer"></param>
        public static void ReadBuffer<T>(SharpDX.Direct3D11.Device dev, Buffer cpuBuffer, Buffer gpuBuffer, ref T[] localBuffer) where T:struct
        {
            DataStream stream;

            // copy the data from GPU
            dev.ImmediateContext.CopyResource(gpuBuffer, cpuBuffer);

            // map the data to be able to read it from CPU
            DataBox outputData = dev.ImmediateContext.MapSubresource(cpuBuffer, D3D.MapMode.Read, D3D.MapFlags.None, out stream);

            // read the data from the gpu
            for (int i = 0; i < localBuffer.Length; i++)
                localBuffer[i] = stream.Read<T>();

            // unmap the data
            dev.ImmediateContext.UnmapSubresource(cpuBuffer, 0);
        }

        /// <summary>
        /// Read the GPU buffer to local array.
        /// The buffer must be a staging buffer ....
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dev"></param>
        /// <param name="cpuBuffer"></param>
        /// <param name="gpuBuffer"></param>
        /// <param name="localBuffer"></param>
        public static void ReadBufferVector<T>(SharpDX.Direct3D11.Device dev, Buffer cpuBuffer, Buffer gpuBuffer, ref IVertex<T>[] localBuffer) where T : struct , IComparable<T>
        {
            DataStream stream;

            // copy the data from GPU
            dev.ImmediateContext.CopyResource(gpuBuffer, cpuBuffer);

            // map the data to be able to read it from CPU
            DataBox outputData = dev.ImmediateContext.MapSubresource(cpuBuffer, D3D.MapMode.Read, D3D.MapFlags.None, out stream);

            // read the data from the gpu
            for (int i = 0; i < localBuffer.Length; i++)
            {
                localBuffer[i] = (IVertex<T>)FxMaths.Vector.FxVector2f.sReadFromDataStream(stream);
            }

            // unmap the data
            dev.ImmediateContext.UnmapSubresource(cpuBuffer, 0);
        }

        public static void WriteBufferStuct<T>(SharpDX.Direct3D11.Device dev, Buffer cpuBuffer, Buffer gpuBuffer, T[] inputBuffer) where T : struct
        {
            DataStream stream;

            // map the data to be able to read it from CPU
            DataBox outputData = dev.ImmediateContext.MapSubresource(cpuBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None, out stream);

            // read the data from the gpu
            for (int i = 0; i < inputBuffer.Length; i++)
                stream.Write<T>(inputBuffer[i]);

            // unmap the data
            dev.ImmediateContext.UnmapSubresource(cpuBuffer, 0);


            // copy the data to GPU
            dev.ImmediateContext.CopyResource(cpuBuffer, gpuBuffer);
        }

        public static void WriteBufferVertex<T>(SharpDX.Direct3D11.Device dev, Buffer cpuBuffer, Buffer gpuBuffer, ICollection<IVertex<T>> inputBuffer) where T : struct , IComparable<T>
        {
            DataStream stream;

            // map the data to be able to read it from CPU
            DataBox outputData = dev.ImmediateContext.MapSubresource(cpuBuffer, D3D.MapMode.Write, D3D.MapFlags.None, out stream);

            // read the data from the gpu
            foreach (var vec in inputBuffer)
                vec.WriteToDataStream(stream);

            
            // unmap the data
            dev.ImmediateContext.UnmapSubresource(cpuBuffer, 0);

            // copy the data to GPU
            dev.ImmediateContext.CopyResource(cpuBuffer ,gpuBuffer);

        }
        #endregion

    }
}
