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
    public class FXEffect : IDisposable
    {
        #region Variables

        #region Bytecode

        /// <summary>
        /// The bytecode of pixel shader
        /// </summary>
        private ShaderBytecode PixelShaderByteCode;

        /// <summary>
        /// The bytecode of vertex  shader
        /// </summary>
        private ShaderBytecode VertexShaderByteCode;

        /// <summary>
        /// The bytecode of compute shader
        /// </summary>
        private ShaderBytecode ComputeShaderByteCode;

        /// <summary>
        /// The bytecode of geometry  shader
        /// </summary>
        private ShaderBytecode GeometryShaderByteCode;

        #endregion


        #region shaders

        /// <summary>
        /// Pixel Shader
        /// </summary>
        public PixelShader PixelShader;

        /// <summary>
        /// Vertex Shader
        /// </summary>
        public VertexShader VertexShader;

        /// <summary>
        /// Compute Shader
        /// </summary>
        public ComputeShader ComputeShader;

        /// <summary>
        /// Geometry Shader
        /// </summary>
        public GeometryShader GeometryShader;

        #endregion


        /// <summary>
        /// The 3d device
        /// </summary>
        private Device device;


        #region ShaderReflection

        /// <summary>
        /// The reflection of the pixel shader.
        /// </summary>
        public ShaderReflection PixelShaderReflection;


        /// <summary>
        /// The reflection of the Vertex shader.
        /// </summary>
        public ShaderReflection VertexShaderReflection;

        /// <summary>
        /// The reflection of the Compute shader.
        /// </summary>
        public ShaderReflection ComputeShaderReflection;


        /// <summary>
        /// The reflection of the Geometry shader.
        /// </summary>
        public ShaderReflection GeometryShaderReflection;

        #endregion


        #region List of constant buffer

        /// <summary>
        /// List with constant Buffers of pixel shader
        /// </summary>
        private List<IFXResources> ListWithConstantBufferOfPixel;

        /// <summary>
        /// List with constant Buffers of vertex shader
        /// </summary>
        private List<IFXResources> ListWithConstantBufferOfVertex;

        /// <summary>
        /// List with constant Buffers of Compute shader
        /// </summary>
        private List<IFXResources> ListWithConstantBufferOfCompute;

        /// <summary>
        /// List with constant Buffers of Geometry shader
        /// </summary>
        private List<IFXResources> ListWithConstantBufferOfGeometry;

        #endregion

        /// <summary>
        /// Which shaders have be include in this effect.
        /// </summary>
        private ShaderType IncludedShaders = ShaderType.None;

        #endregion



        #region Constractor

        public FXEffect( Device dev, ShaderBytecode psByteCode = null, ShaderBytecode vsByteCode = null, ShaderBytecode csByteCode = null, ShaderBytecode gsByteCode = null )
        {
            // store the bytecodes
            this.PixelShaderByteCode = psByteCode;
            this.VertexShaderByteCode = vsByteCode;
            this.ComputeShaderByteCode = csByteCode;
            this.GeometryShaderByteCode = gsByteCode;

            #region init Vertex 
            // check for the shader
            if ( vsByteCode != null ) {
                // set the flag for the shader
                IncludedShaders |= ShaderType.Vertex;
                
                // init the shader
                this.VertexShader = new VertexShader( dev, vsByteCode );

                // create  the reflection of shaders
                VertexShaderReflection = new ShaderReflection( vsByteCode );

                // init the lists
                ListWithConstantBufferOfVertex = new List<IFXResources>();
            }
            #endregion



            #region init Pixel

            // check for the shader
            if ( psByteCode != null ) {
                // set the flag for the shader
                IncludedShaders |= ShaderType.Pixel;

                // init the shader
                this.PixelShader = new PixelShader( dev, psByteCode );

                // create  the reflection of shaders
                PixelShaderReflection = new ShaderReflection( psByteCode );

                // init the lists
                ListWithConstantBufferOfPixel = new List<IFXResources>();
            }

            #endregion



            #region init Compute

            // check for the shader
            if ( csByteCode != null ) {
                // set the flag for the shader
                IncludedShaders |= ShaderType.Compute;

                try
                {
                    
                    // init the shader
                    this.ComputeShader = new ComputeShader(dev, csByteCode);

                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                    System.Windows.Forms.MessageBox.Show(ex.StackTrace);
                    System.Windows.Forms.MessageBox.Show(ex.Source);
                }

                // create  the reflection of shaders
                ComputeShaderReflection = new ShaderReflection( csByteCode );

                // init the lists
                ListWithConstantBufferOfCompute = new List<IFXResources>();
            }

            #endregion


            #region init Geometry

            // check for the shader
            if ( gsByteCode != null ) {
                // set the flag for the shader
                IncludedShaders |= ShaderType.Geometry;

                // init the shader
                GeometryShader = new GeometryShader( dev, gsByteCode );

                // create  the reflection of shaders
                GeometryShaderReflection = new ShaderReflection( gsByteCode );

                // init the lists
                ListWithConstantBufferOfGeometry = new List<IFXResources>();

            }

            #endregion


            // store the device
            this.device = dev;

        }

        #endregion



        #region Get

        public FXConstantBuffer GetConstantBufferByName( String Name )
        {
            /// create the buffer base on the name
            FXConstantBuffer tmpCB = new FXConstantBuffer( device, Name, PixelShaderReflection, VertexShaderReflection, ComputeShaderReflection, GeometryShaderReflection );

            /// add the new cb to the list to use it when we execute the shader
            /// insert the cb in the lists that they are going to use it
            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Pixel ) )
                ListWithConstantBufferOfPixel.Add( tmpCB );

            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Vertex ) )
                ListWithConstantBufferOfVertex.Add( tmpCB );

            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Compute ) )
                ListWithConstantBufferOfCompute.Add( tmpCB );

            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Geometry ) )
                ListWithConstantBufferOfGeometry.Add( tmpCB );
            return tmpCB;
        }

        public FXConstantBuffer<T> GetConstantBufferByName<T>( String Name ) where T:struct
        {
            /// create the buffer base on the name
            FXConstantBuffer<T> tmpCB = new FXConstantBuffer<T>( device, Name, PixelShaderReflection, VertexShaderReflection, ComputeShaderReflection, GeometryShaderReflection );

            /// add the new cb to the list to use it when we execute the shader
            /// insert the cb in the lists that they are going to use it
            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Pixel ) )
                ListWithConstantBufferOfPixel.Add( tmpCB );

            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Vertex ) )
                ListWithConstantBufferOfVertex.Add( tmpCB );

            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Compute ) )
                ListWithConstantBufferOfCompute.Add( tmpCB );

            if ( tmpCB.WhereIsExist.HasFlag( ShaderType.Geometry ) )
                ListWithConstantBufferOfGeometry.Add( tmpCB );

            if (tmpCB != null && tmpCB.Buffer != null)
                tmpCB.Buffer.DebugName = Name;
            return tmpCB;
        }


        public FXResourceVariable GetResourceByName( string Name )
        {
            /// create the buffer base on the name
            FXResourceVariable tmpRV = new FXResourceVariable( Name, PixelShaderReflection, VertexShaderReflection , ComputeShaderReflection, GeometryShaderReflection );
            
            /// add the new cb to the list to use it when we execute the shader
            /// insert the cb in the lists that they are going to use it
            if ( tmpRV.WhereIsExist.HasFlag( ShaderType.Pixel ) )
                ListWithConstantBufferOfPixel.Add( tmpRV );

            if ( tmpRV.WhereIsExist.HasFlag( ShaderType.Vertex ) )
                ListWithConstantBufferOfVertex.Add( tmpRV );

            if ( tmpRV.WhereIsExist.HasFlag( ShaderType.Compute ) )
                ListWithConstantBufferOfCompute.Add( tmpRV );

            if ( tmpRV.WhereIsExist.HasFlag( ShaderType.Geometry ) )
                ListWithConstantBufferOfGeometry.Add( tmpRV );
            return tmpRV;
        }

        #endregion



        #region Apply

        public void Apply(DeviceContext deviceContext)
        {
            // =====================  set the shaders =====================

            // --------  set the Pixel shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Pixel))
            {
                // set the shader
                deviceContext.PixelShader.Set(PixelShader);

                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfPixel)
                    cb.Commit(deviceContext, ShaderType.Pixel);
            }
            else
            {
                deviceContext.PixelShader.Set(null);
            }

            // --------  set the Vertex shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Vertex))
            {
                // set the shader
                deviceContext.VertexShader.Set(VertexShader);

                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfVertex)
                    cb.Commit(deviceContext, ShaderType.Vertex);
            }
            else
            {
                deviceContext.VertexShader.Set(null);
            }

            // --------  set the Compute shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Compute))
            {
                // set the shader
                deviceContext.ComputeShader.Set(ComputeShader);

                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfCompute)
                    cb.Commit(deviceContext, ShaderType.Compute);
            }
            else
            {
                deviceContext.ComputeShader.Set(null);
            }


            // --------  set the Geometry shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Geometry))
            {
                // set the shader
                deviceContext.GeometryShader.Set(GeometryShader);

                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfGeometry)
                    cb.Commit(deviceContext, ShaderType.Geometry);
            }
            else
            {
                deviceContext.GeometryShader.Set(null);
            }



        }

        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            // Pixel
            PixelShader.Dispose();
            PixelShaderByteCode.Dispose();
            PixelShaderReflection.Dispose();

            // vertex
            VertexShader.Dispose();
            VertexShaderByteCode.Dispose();
            VertexShaderReflection.Dispose();

            // Compute
            ComputeShader.Dispose();
            ComputeShaderByteCode.Dispose();

            // Geometry
            GeometryShader.Dispose();
            GeometryShaderByteCode.Dispose();

            // CB
            foreach ( IFXResources cb in ListWithConstantBufferOfPixel )
                cb.Dispose();

            foreach ( IFXResources cb in ListWithConstantBufferOfVertex )
                cb.Dispose();

            foreach ( IFXResources cb in ListWithConstantBufferOfCompute )
                cb.Dispose();

            foreach ( IFXResources cb in ListWithConstantBufferOfGeometry )
                cb.Dispose();

        }

        #endregion


        #region Clean Binding


        public void CleanBind(DeviceContext deviceContext)
        {
            // =====================  set the shaders =====================

            // --------  set the Pixel shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Pixel))
            {
                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfPixel)
                    cb.CleanBind(deviceContext, ShaderType.Pixel);
            }

            // --------  set the Vertex shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Vertex))
            {
                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfVertex)
                    cb.CleanBind(deviceContext, ShaderType.Vertex);
            }

            // --------  set the Compute shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Compute))
            {
                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfCompute)
                    cb.CleanBind(deviceContext, ShaderType.Compute);
            }

            // --------  set the Geometry shader ----------

            if (IncludedShaders.HasFlag(ShaderType.Geometry))
            {
                // set the buffers
                foreach (IFXResources cb in ListWithConstantBufferOfGeometry)
                    cb.CleanBind(deviceContext, ShaderType.Geometry);
            }
        }

        #endregion
    }
}
