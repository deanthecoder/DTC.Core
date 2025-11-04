// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System.IO;
using System.Linq;
using System.Reflection;
using DTC.Core.Extensions;

namespace DTC.Core.UnitTesting;

public abstract class TestsBase
{
    // ReSharper disable once InconsistentNaming
    private static DirectoryInfo m_projectDir;
    
    protected static DirectoryInfo ProjectDir
    {
        get
        {
            if (m_projectDir == null)
            {
                var location = Assembly.GetExecutingAssembly().Location;
                m_projectDir = location.ToFile().Directory;
                while (m_projectDir?.EnumerateFiles("*.csproj").Any() != true && m_projectDir?.Parent != null)
                    m_projectDir = m_projectDir.Parent;
            }
            
            return m_projectDir;
        }
    }
}