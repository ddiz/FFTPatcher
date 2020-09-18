﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EntryEdit.Editors
{
    public partial class ConditionalSetsEditor : UserControl
    {
        private List<ConditionalSet> _conditionalSets;
        private List<ConditionalSet> _defaultConditionalSets;

        private int _conditionalSetIndex = 0;
        private bool _isPopulate = false;

        public ConditionalSetsEditor()
        {
            InitializeComponent();
        }

        public void Populate(List<ConditionalSet> conditionalSets, List<ConditionalSet> defaultConditionalSets, CommandData commandData)
        {
            this._conditionalSets = conditionalSets;
            this._defaultConditionalSets = defaultConditionalSets;

            conditionalSetEditor.Init(commandData);

            PopulateSets();
        }

        public void PopulateSets(int index = 0, bool reloadSet = true)
        {
            _isPopulate = true;

            if (_conditionalSets.Count > 0)
            {
                cmb_ConditionalSet.Items.Clear();
                cmb_ConditionalSet.Items.AddRange(_conditionalSets.ToArray());
                cmb_ConditionalSet.SelectedIndex = index;
                SetConditionalSetIndex(index, reloadSet);
                btn_Delete.Enabled = true;
            }
            else
            {
                ClearSet();
            }

            _isPopulate = false;
        }

        private void SetConditionalSetIndex(int index, bool reloadSet = true)
        {
            _conditionalSetIndex = index;

            if (reloadSet)
                conditionalSetEditor.Populate(_conditionalSets[index], EntryData.GetEntry<ConditionalSet>(_defaultConditionalSets, index));
        }

        private void ClearSet()
        {
            cmb_ConditionalSet.Items.Clear();
            cmb_ConditionalSet.SelectedIndex = -1;
            _conditionalSetIndex = -1;
            conditionalSetEditor.ClearBlock();
            conditionalSetEditor.SetEnabledState(false);
            btn_Delete.Enabled = false;
        }

        private void SwapSetByOffset(int offset)
        {
            if (PatcherLib.Utilities.Utilities.SafeSwap<ConditionalSet>(_conditionalSets, _conditionalSetIndex, _conditionalSetIndex + offset))
            {
                _conditionalSets[_conditionalSetIndex].AddOffsetToIndex(-offset);
                _conditionalSets[_conditionalSetIndex + offset].AddOffsetToIndex(offset);
            }
        }

        private void Clear()
        {
            if (_conditionalSets[_conditionalSetIndex].ConditionalBlocks.Count > 0)
            {
                _conditionalSets[_conditionalSetIndex].ConditionalBlocks.Clear();
                PopulateSets(_conditionalSetIndex);
            }
        }

        private void Reload()
        {
            if ((_defaultConditionalSets != null) && (_conditionalSetIndex < _defaultConditionalSets.Count))
            {
                _conditionalSets[_conditionalSetIndex].ConditionalBlocks.Clear();
                _conditionalSets[_conditionalSetIndex].ConditionalBlocks.AddRange(CopyableEntry.CopyList<ConditionalBlock>(_defaultConditionalSets[_conditionalSetIndex].ConditionalBlocks));
                PopulateSets(_conditionalSetIndex);
            }
        }

        private void cmb_ConditionalSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_ConditionalSet.SelectedIndex != _conditionalSetIndex)
            {
                if (!_isPopulate)
                {
                    conditionalSetEditor.SaveBlock();
                    SetConditionalSetIndex(cmb_ConditionalSet.SelectedIndex);
                }
            }
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            if (_conditionalSets.Count > 0)
            {
                _conditionalSets.RemoveAt(_conditionalSetIndex);
                if (_conditionalSets.Count > 0)
                {
                    bool isFirstIndex = (_conditionalSetIndex > 0);
                    int newIndex = isFirstIndex ? (_conditionalSetIndex - 1) : 0;
                    int startIndex = isFirstIndex ? (newIndex + 1) : 0;

                    for (int index = startIndex; index < _conditionalSets.Count; index++)
                        _conditionalSets[index].DecrementIndex();

                    PopulateSets(newIndex);
                }
                else
                {
                    ClearSet();
                }
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            conditionalSetEditor.SaveBlock();

            int newIndex = _conditionalSetIndex + 1;
            _conditionalSets.Insert(newIndex, new ConditionalSet(newIndex, string.Empty, new List<ConditionalBlock>()));

            for (int index = newIndex + 1; index < _conditionalSets.Count; index++)
                _conditionalSets[index].IncrementIndex();

            PopulateSets(newIndex);
        }

        private void btn_Up_Click(object sender, EventArgs e)
        {
            if (_conditionalSetIndex > 0)
            {
                SwapSetByOffset(-1);
                PopulateSets(_conditionalSetIndex - 1, false);
            }
        }

        private void btn_Down_Click(object sender, EventArgs e)
        {
            if (_conditionalSetIndex < (_conditionalSets.Count - 1))
            {
                SwapSetByOffset(1);
                PopulateSets(_conditionalSetIndex + 1, false);
            }
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void btn_Reload_Click(object sender, EventArgs e)
        {
            Reload();
        }
    }
}
