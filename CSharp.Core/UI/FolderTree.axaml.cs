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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;
using JetBrains.Annotations;

namespace CSharp.Core.UI;

public partial class FolderTree : UserControl
{
    [UsedImplicitly]
    private ObservableCollection<FolderTreeNode> Nodes { get; } = [];
    private DirectoryInfo m_location;
    private ISelectedItemsProvider<DirectoryInfo> m_selectedItemsProvider;
    public static readonly DirectProperty<FolderTree, ISelectedItemsProvider<DirectoryInfo>> SelectedItemsProviderProperty = AvaloniaProperty.RegisterDirect<FolderTree, ISelectedItemsProvider<DirectoryInfo>>(nameof(SelectedItemsProvider), o => o.SelectedItemsProvider, (o, v) => o.SelectedItemsProvider = v);

    public static readonly DirectProperty<FolderTree, DirectoryInfo> LocationProperty =
        AvaloniaProperty.RegisterDirect<FolderTree, DirectoryInfo>(nameof(Location), o => o.Location, (o, v) => o.Location = v);

    public FolderTree()
    {
        InitializeComponent();
    }

    public DirectoryInfo Location
    {
        get => m_location;
        set
        {
            if (SetAndRaise(LocationProperty, ref m_location, value))
            {
                Nodes.Clear();
                if (value == null)
                    return;
                var node = new FolderTreeNode { Directory = value, IsExpanded = true, IsSelected = true};
                node.SelectionChanged += (_, _) => SelectedItemsProvider?.SetSelectedItems(node.GetAllSelectedItems().ToArray());
                Nodes.Add(node);

                SelectedItemsProvider?.SetSelectedItems(node.GetAllSelectedItems().ToArray());
            }
        }
    }
    public ISelectedItemsProvider<DirectoryInfo> SelectedItemsProvider
    {
        get => m_selectedItemsProvider;
        set => SetAndRaise(SelectedItemsProviderProperty, ref m_selectedItemsProvider, value);
    }

    [DebuggerDisplay("{Directory.Name} (IsSelected={IsSelected}, IsExpanded={IsExpanded})")]
    public class FolderTreeNode : ViewModelBase
    {
        private readonly DirectoryInfo m_directory;
        private bool m_isExpanded;
        private bool? m_isSelected = false;

        private FolderTreeNode Parent { get; init; }
        
        public event EventHandler SelectionChanged;

        public ObservableCollection<FolderTreeNode> SubFolders { get; } = [];

        public bool? IsSelected
        {
            get => m_isSelected;
            set
            {
                var newValue = value;
                if (m_isSelected == true && value == null)
                    newValue = false;
                
                if (!SetField(ref m_isSelected, newValue))
                    return;

                if (newValue.HasValue)
                {
                    EnsureChildrenPopulated();
                    SubFolders.ToList().ForEach(o => o.IsSelected = newValue);
                }

                if (newValue == false && Parent?.IsSelected == true || newValue == true && Parent?.IsSelected == false)
                {
                    // Walk all parents.
                    var parent = Parent;
                    while (parent != null)
                    {
                        parent.SetIsSelected(null);
                        parent = parent.Parent;
                    }
                }
                
                // Notify the root the selection has changed.
                var p = this;
                while (p.Parent != null)
                    p = p.Parent;
                p.SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetIsSelected(bool? b) => SetField(ref m_isSelected, b, nameof(IsSelected));

        public bool IsExpanded
        {
            get => m_isExpanded;
            set
            {
                if (!SetField(ref m_isExpanded, value))
                    return;

                if (value)
                    EnsureChildrenPopulated();
            }
        }

        private void EnsureChildrenPopulated()
        {
            if (SubFolders.FirstOrDefault() != null)
                return; // Already populated.
            
            SubFolders.Clear();
            foreach (var node in GetDirs().Select(o =>
                     {
                         var node = new FolderTreeNode
                         {
                             Parent = this, Directory = o
                         };
                         if (IsSelected.HasValue)
                             node.IsSelected = IsSelected;
                         return node;
                     }).ToArray())
            {
                SubFolders.Add(node);
            }
        }

        public DirectoryInfo Directory
        {
            get => m_directory;
            init
            {
                m_directory = value;
                if (GetDirs().Length > 0)
                {
                    // Add placeholder child, populated upon tree expansion.
                    SubFolders.Add(null);
                }
            }
        }

        private DirectoryInfo[] GetDirs()
        {
            var dirs = Directory.TryGetDirs();
            return dirs.Where(o => !o.Name.StartsWith('.') && !o.Attributes.HasFlag(FileAttributes.Hidden)).ToArray();
        }

        public IEnumerable<DirectoryInfo> GetAllSelectedItems()
        {
            if (IsSelected == true)
            {
                yield return Directory;
                yield break;
            }
            
            foreach (var folderNode in SubFolders)
            {
                if (folderNode == null)
                    continue;

                if (folderNode.IsSelected == true)
                {
                    yield return folderNode.Directory;
                }
                else if (folderNode.IsSelected == null)
                {
                    // Partial selection — recurse into children
                    foreach (var child in folderNode.GetAllSelectedItems())
                        yield return child;
                }
            }
        }
    }
}

public interface ISelectedItemsProvider<T>
{
    void SetSelectedItems(T[] items);
}

