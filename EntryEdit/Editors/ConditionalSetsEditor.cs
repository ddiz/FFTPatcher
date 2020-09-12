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

        private List<string> _commandNames;
        private int _conditionalSetIndex = 0;
        private bool _isPopulate = false;

        public ConditionalSetsEditor()
        {
            InitializeComponent();
        }

        public void Populate(List<ConditionalSet> conditionalSets, List<ConditionalSet> defaultConditionalSets,  List<string> commandNames, Dictionary<string, Dictionary<int, string>> parameterValueMaps, int maxParameters)
        {
            _isPopulate = true;

            this._conditionalSets = conditionalSets;
            this._defaultConditionalSets = defaultConditionalSets;
            this._commandNames = commandNames;

            conditionalSetEditor.Init(commandNames, parameterValueMaps, maxParameters);

            cmb_ConditionalSet.Items.Clear();
            cmb_ConditionalSet.Items.AddRange(_conditionalSets.ToArray());
            cmb_ConditionalSet.SelectedIndex = 0;

            SetConditionalSetIndex(0);
            _isPopulate = false;
        }

        private void SetConditionalSetIndex(int index)
        {
            _conditionalSetIndex = index;
            conditionalSetEditor.Populate(_conditionalSets[index], EntryData.GetEntry<ConditionalSet>(_defaultConditionalSets, index));
        }

        private void cmb_ConditionalSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isPopulate)
                SetConditionalSetIndex(cmb_ConditionalSet.SelectedIndex);
        }
    }
}
