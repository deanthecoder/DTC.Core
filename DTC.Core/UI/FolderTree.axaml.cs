// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;

namespace DTC.Core.UI;

public partial class FolderTree : UserControl
{
    private FolderTreeRoot m_root;
    public static readonly DirectProperty<FolderTree, FolderTreeRoot> RootProperty = AvaloniaProperty.RegisterDirect<FolderTree, FolderTreeRoot>(nameof(Root), o => o.Root, (o, v) => o.Root = v);

    [UsedImplicitly]
    private ObservableCollection<FolderTreeNode> Nodes { get; } = [];

    public FolderTreeRoot Root
    {
        get => m_root;
        set
        {
            if (!SetAndRaise(RootProperty, ref m_root, value))
                return; // No change.

            Nodes.Clear();
            if (value == null)
            {
                m_root.RootNode = null;
                return;
            }
            
            var node = new FolderTreeNode { Directory = value.Root, IsSelected = true, IsExpanded = true};
            Nodes.Add(node);

            m_root.RootNode = node;
        }
    }

    public FolderTree()
    {
        InitializeComponent();
    }
}