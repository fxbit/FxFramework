using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

// resolve conflict - DXGI.Device & Direct3D10.Device
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;
using Effect = SlimDX.Direct3D11.Effect;
using EffectFlags = SlimDX.D3DCompiler.EffectFlags;



namespace FXFramework
{
    [Flags]
    public enum ShaderType { None = 0x00, Pixel = 0x01, Vertex = 0x02, Compute = 0x04, Geometry = 0x08 };


    /// <summary>
    /// The type of the buffer
    /// </summary>
    [Flags]
    public enum AccessViewType { SRV = 0x00, UAV = 0x01 };

    public interface IFXResources
    {
        /// <summary>
        /// Commit any memmory changes and set the buffers
        /// </summary>
        /// <param name="deviceContext"></param>
        /// <param name="type"></param>
        void Commit( DeviceContext deviceContext, ShaderType type );

                /// <summary>
        /// Clean the resource binding
        /// </summary>
        /// <param name="deviceContext"></param>
        /// <param name="type"></param>
        void CleanBind(DeviceContext deviceContext, ShaderType type);

        void Dispose();
    }


    public interface IFXVariable
    {
        /// <summary>
        /// Update the data in datastream of the constant buffer if the data are dirty.
        /// </summary>
        /// <returns>If we have change</returns>
        Boolean Commit();

        /// <summary>
        /// We have changes to the buffer ?
        /// </summary>
        Boolean isDirty { get; }
    }

}
