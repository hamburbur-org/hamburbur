using UnityEngine;
// ReSharper disable ShaderLabShaderReferenceNotResolved

namespace hamburbur.Tools;

public abstract class Shaders
{
    public static Shader UberShader
    {
        get
        {
            if (field == null)
                field = Shader.Find("GorillaTag/UberShader");
                
            return field;
        }
    }
    
    public static Shader TextShader
    {
        get
        {
            if (field == null)
                field = Shader.Find("GUI/Text Shader");
                
            return field;
        }
    }
}