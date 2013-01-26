#define FXMaths

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX.D3DCompiler;
using SharpDX;

#if FXMaths
using FxMaths.Vector;
#endif

using System.Runtime.InteropServices;

namespace FXFramework
{
    public static class Utils
    {

        /// <summary>
        /// Check if the variable type in c# is compatible with the type in shader
        /// </summary>
        /// <param name="c_sharp_type"></param>
        /// <param name="shaderType"></param>
        /// <returns></returns>
        public static bool CheckCompatibilityOfType<T>( T c_sharp_type, ShaderTypeDescription varDesc ) where T : struct
        {
            /// check if the class is matrix
            if ( varDesc.Class == ShaderVariableClass.MatrixColumns ) {

                #region SharpDX Matrix
                if ( c_sharp_type is Matrix ) {
                    return ( varDesc.Type == ShaderVariableType.Float );
                }
                #endregion

            }

            /// check if is vector
            if ( varDesc.Class == ShaderVariableClass.Vector ) {

                #region SharpDX vectors
                if ( c_sharp_type is Vector2 ) {
                    return (varDesc.Type == ShaderVariableType.Float) && varDesc.ColumnCount == 2;
                }

                if ( c_sharp_type is Vector3 ) {
                    return (varDesc.Type == ShaderVariableType.Float) && varDesc.ColumnCount == 3;
                }

                if ( c_sharp_type is Vector4 ) {
                    return (varDesc.Type == ShaderVariableType.Float) && varDesc.ColumnCount == 4;
                }
                #endregion

                #if FXMaths

                #region FxVector float
                if ( c_sharp_type is FxVector2f ) {
                    return (varDesc.Type == ShaderVariableType.Float) && varDesc.ColumnCount == 2;
                }

                if ( c_sharp_type is FxVector3f ) {
                    return (varDesc.Type == ShaderVariableType.Float) && varDesc.ColumnCount == 3;
                }

                if ( c_sharp_type is FxVector4f ) {
                    return (varDesc.Type == ShaderVariableType.Float) && varDesc.ColumnCount == 4;
                }
                #endregion


                #region FxVector int
                if ( c_sharp_type is FxVector2i ) {
                    return (varDesc.Type == ShaderVariableType.Int) && varDesc.ColumnCount == 2;
                }

                if ( c_sharp_type is FxVector3i ) {
                    return (varDesc.Type == ShaderVariableType.Int) && varDesc.ColumnCount == 3;
                }

                if ( c_sharp_type is FxVector4i ) {
                    return (varDesc.Type == ShaderVariableType.Int) && varDesc.ColumnCount == 4;
                }
                #endregion


                #region FxVector float
                if ( c_sharp_type is FxVector2b ) {
                    return (varDesc.Type == ShaderVariableType.UInt8) && varDesc.ColumnCount == 2;
                }

                if ( c_sharp_type is FxVector3b ) {
                    return (varDesc.Type == ShaderVariableType.UInt8) && varDesc.ColumnCount == 3;
                }

                if ( c_sharp_type is FxVector4b ) {
                    return (varDesc.Type == ShaderVariableType.UInt8) && varDesc.ColumnCount == 4;
                }
                #endregion

                #endif

            }

            /// check if the class is scalar
            if ( varDesc.Class == ShaderVariableClass.Scalar ) {

                #region c# scalar 

                // find the type of variable
                if ( c_sharp_type is int || c_sharp_type is Int32 ) {
                    return ( varDesc.Type == ShaderVariableType.Int );
                }

                if ( c_sharp_type is float) {
                    return ( varDesc.Type == ShaderVariableType.Float );
                }

                if ( c_sharp_type is double ) {
                    return ( varDesc.Type == ShaderVariableType.Double );

                }
                if ( c_sharp_type is uint ) {
                    return ( varDesc.Type == ShaderVariableType.UInt );
                }

                if ( c_sharp_type is byte) {
                    return ( varDesc.Type == ShaderVariableType.UInt8 );
                }

                #endregion

            }

            if ( varDesc.Class == ShaderVariableClass.Struct ) {

                // get the size of the struct of c#
                int size = Marshal.SizeOf( typeof( T ) )/4; // in 4byte form

                // check the size 
                if (size == varDesc.ColumnCount)
                    return true;
                else
                    return false;
                
            }

            throw new System.ApplicationException( "The type is not exist in the function.... something is not right!!!" );

            return false;
        }

    }
}
