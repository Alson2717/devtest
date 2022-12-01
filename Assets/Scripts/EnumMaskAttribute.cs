using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnumMaskAttribute : PropertyAttribute
{
    public Type EnumType;
    public Enum Enum;
    public EnumMaskAttribute(Type enumType)
    {
        this.EnumType = enumType;
        this.Enum = (Enum)Enum.GetValues(enumType).GetValue(0);
    }
}