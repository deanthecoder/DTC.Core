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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace CSharp.Core.UI;

public class FolderTreeRoot
{
    private FolderTreeNode m_rootNode;
    
    public event EventHandler SelectionChanged;
    
    [NotNull] public DirectoryInfo Root { get; }

    public FolderTreeNode RootNode
    {
        get => m_rootNode;
        set
        {
            if (m_rootNode == value)
                return;
            if (m_rootNode != null)
                m_rootNode.SelectionChanged -= OnSelectionChanged;
            m_rootNode = value;
            if (m_rootNode != null)
                m_rootNode.SelectionChanged += OnSelectionChanged;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public FolderTreeRoot([NotNull] DirectoryInfo root)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public IEnumerable<DirectoryInfo> GetSelectedItems() =>
        RootNode?.GetAllSelectedItems() ?? Array.Empty<DirectoryInfo>();

    private void OnSelectionChanged(object sender, EventArgs e) =>
        SelectionChanged?.Invoke(this, e);
}