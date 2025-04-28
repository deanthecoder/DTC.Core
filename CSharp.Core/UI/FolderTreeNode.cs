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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;

namespace CSharp.Core.UI;

[DebuggerDisplay("{Directory.Name} (IsSelected={IsSelected}, IsExpanded={IsExpanded})")]
public class FolderTreeNode : ViewModelBase
{
    private readonly DirectoryInfo m_directory;
    private bool m_isExpanded;
    private bool? m_isSelected = false;

    private FolderTreeNode Parent { get; init; }
        
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
                SelectAllChildren(newValue.Value);
            
            // Walk all parents.
            var parent = Parent;
            while (parent != null)
            {
                var mixedChildren = parent.SubFolders.Select(o => o.IsSelected).Distinct().Count() > 1;
                var allOn = !mixedChildren && parent.SubFolders.All(o => o.IsSelected == true);
                var allOff = !allOn && parent.SubFolders.All(o => o.IsSelected == false);
                if (allOn)
                    parent.SetField(ref parent.m_isSelected, true);
                else if (allOff)
                    parent.SetField(ref parent.m_isSelected, false);
                else if (mixedChildren)
                    parent.SetField(ref parent.m_isSelected, null);
                parent = parent.Parent;
            }
        }
    }

    private void SelectAllChildren(bool b)
    {
        foreach (var node in SubFolders)
        {
            if (node == null)
                continue;
            node.IsSelected = b;
            node.SelectAllChildren(b);
        }
    }

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
        GetDirs()
            .Select(o =>
            {
                var node = new FolderTreeNode
                {
                    Parent = this,
                    Directory = o
                };
                if (IsSelected.HasValue)
                    node.m_isSelected = IsSelected;
                return node;
            })
            .ForEach(o => SubFolders.Add(o));
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