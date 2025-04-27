// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using System;
using System.IO;
using JetBrains.Annotations;

namespace CSharp.Core.UI;

public class FolderTreeRoot
{
    private DirectoryInfo[] m_selectedItems;

    [NotNull] public DirectoryInfo Root { get; }

    public void SetSelectedItems(DirectoryInfo[] items) =>
        m_selectedItems = items;

    public DirectoryInfo[] GetSelectedItems() =>
        m_selectedItems ?? [];

    public FolderTreeRoot([NotNull] DirectoryInfo root)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
    }
}