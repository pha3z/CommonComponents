using System;
using System.Collections.Generic;
using System.Text;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// TList stores generic structs in a byte array and uses newer C# language features
    /// to apply schema at access time. The consumer must know
    /// what type of struct is stored in the list and use the proper generic parameter to 
    /// make indexed access into the struct.
    /// <br/><br/>
    /// Use this when you want blazing performance of struct storage
    /// but you need to store the data as bytes for which you won't know the type until you access them.
    /// </summary>
    public class TList
    {

    }
}
