/*
    Copyright 2007, Joe Davidson <joedavidson@gmail.com>

    This file is part of FFTPatcher.

    FFTPatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FFTPatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FFTPatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using FFTPatcher.Datatypes;
using PatcherLib;

namespace FFTPatcher.Editors
{
    public partial class ElementsEditor : BaseEditor
    {
		#region Instance Variables 

        private Elements defaults;
        
        //private static string[] elementNames = new string[] {
        //        "Fire", "Lightning", "Ice", "Wind", "Earth", "Water", "Holy", "Dark" };
        private Elements elements = new Elements( 0 );

        private bool ignoreChanges = false;

		#endregion Instance Variables 

		#region Public Properties 

        public string GroupBoxText
        {
            get { return elementsGroupBox.Text; }
            set { elementsGroupBox.Text = value; }
        }

		#endregion Public Properties 

		#region Constructors (1) 

        public ElementsEditor()
        {
            InitializeComponent();
            elementsCheckedListBox.ItemCheck += elementsCheckedListBox_ItemCheck;
        }

		#endregion Constructors 

		#region Public Methods

        public void SetValueAndDefaults( Elements value, Elements defaults )
        {
            elements = value;
            this.defaults = defaults;
            Enabled = true;
            UpdateView();
        }

		#endregion Public Methods 

		#region Private Methods 

        private void elementsCheckedListBox_ItemCheck( object sender, ItemCheckEventArgs e )
        {
            if( !ignoreChanges )
            {
                //string s = elementsCheckedListBox.Items[e.Index].ToString();
                //PropertyInfo pi = elements.GetType().GetProperty( s );
                //pi.SetValue( elements, e.NewValue == CheckState.Checked, null );

                //elements[e.Index] 
                byte flag = (byte)(0x80 >> e.Index);
                elements.SetElementFlagState((Elements.ElementFlags)flag, (e.NewValue == CheckState.Checked));
                OnDataChanged( this, System.EventArgs.Empty );
            }
        }

        private void UpdateView()
        {
            this.SuspendLayout();
            elementsCheckedListBox.SuspendLayout();

            ignoreChanges = true;
            //elementsCheckedListBox.SetValuesAndDefaults( ReflectionHelpers.GetFieldsOrProperties<bool>( elements, elementNames ), ReflectionHelpers.GetFieldsOrProperties<bool>( defaults, elementNames ) );
            elementsCheckedListBox.SetValuesAndDefaults(elements.ToBoolArray(), defaults.ToBoolArray());
            ignoreChanges = false;

            elementsCheckedListBox.ResumeLayout();
            this.ResumeLayout();
        }

        #endregion Private Methods


        private class ElementsCheckedListBox : CheckedListBox
        {
            /*
            private enum Elements
            {
                Fire,
                Lightning,
                Ice,
                Wind,
                Earth,
                Water,
                Holy,
                Dark
            }
            */

            private SolidBrush[] elementFGBrushes = null;
            private SolidBrush[] elementBGBrushes = null;

            public bool[] Defaults { get; private set; }
            
            public ElementsCheckedListBox()
                : base()
            {
                CheckOnClick = true;
                GetElements();
                GetBrushes();
            }

            public void SetValuesAndDefaults( bool[] values, bool[] defaults )
            {
                if( (values != null) && (defaults != null) && (this.Defaults == null) )
                {
                    this.Defaults = defaults;
                    for( int i = 0; i < values.Length; i++ )
                    {
                        SetItemChecked( i, values[i] );
                        RefreshItem( i );
                    }
                }
                else if( (values != null) && (defaults != null) && (this.Defaults != null) )
                {
                    List<int> itemsToRefresh = new List<int>( values.Length );
                    for( int i = 0; i < values.Length; i++ )
                    {
                        if( ((GetItemChecked( i ) ^ this.Defaults[i]) && !(values[i] ^ defaults[i])) ||
                            (!(GetItemChecked( i ) ^ this.Defaults[i]) && (values[i] ^ defaults[i])) )
                        {
                            itemsToRefresh.Add( i );
                        }
                    }

                    this.Defaults = defaults;
                    for( int i = 0; i < values.Length; i++ )
                    {
                        SetItemChecked( i, values[i] );
                    }

                    foreach( int i in itemsToRefresh )
                    {
                        SetItemChecked( i, !values[i] );
                        SetItemChecked( i, values[i] );
                    }
                }
            }

            protected override void OnDrawItem( DrawItemEventArgs e )
            {
                /*
                Brush backColorBrush = Brushes.White;
                Brush foreColorBrush = Brushes.Black;

                switch( (Element)e.Index )
                {
                    case Element.Fire:
                        backColorBrush = Brushes.Red;
                        foreColorBrush = Brushes.White;
                        break;
                    case Element.Lightning:
                        backColorBrush = Brushes.Purple; // TODO: find a better color
                        foreColorBrush = Brushes.White;
                        break;
                    case Element.Ice:
                        backColorBrush = Brushes.LightCyan;
                        foreColorBrush = Brushes.Black;
                        break;
                    case Element.Wind:
                        backColorBrush = Brushes.Yellow;
                        foreColorBrush = Brushes.Black;
                        break;
                    case Element.Earth:
                        backColorBrush = Brushes.Green;
                        foreColorBrush = Brushes.White;
                        break;
                    case Element.Water:
                        backColorBrush = Brushes.LightBlue;
                        foreColorBrush = Brushes.Black;
                        break;
                    case Element.Holy:
                        backColorBrush = Brushes.White;
                        foreColorBrush = Brushes.Black;
                        break;
                    case Element.Dark:
                        backColorBrush = Brushes.Black;
                        foreColorBrush = Brushes.White;
                        break;
                    default:
                        // empty
                        break;
                }
                */

                Brush backColorBrush = (e.Index < elementBGBrushes.Length ? elementBGBrushes[e.Index] : Brushes.White);
                Brush foreColorBrush = (e.Index < elementFGBrushes.Length ? elementFGBrushes[e.Index] : Brushes.Black);

                e.Graphics.FillRectangle( backColorBrush, e.Bounds );
                CheckBoxState state = this.GetItemChecked( e.Index ) ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                Size checkBoxSize = CheckBoxRenderer.GetGlyphSize( e.Graphics, state );
                Point loc = new Point( 1, (e.Bounds.Height - (checkBoxSize.Height + 1)) / 2 + 1 );
                CheckBoxRenderer.DrawCheckBox( e.Graphics, new Point( loc.X + e.Bounds.X, loc.Y + e.Bounds.Y ), state );
                e.Graphics.DrawString( this.Items[e.Index].ToString(), e.Font, foreColorBrush, new PointF( loc.X + checkBoxSize.Width + 1 + e.Bounds.X, loc.Y + e.Bounds.Y ) );
                
                if( (Defaults != null) && (Defaults.Length > e.Index) && (Defaults[e.Index] != GetItemChecked( e.Index )) )
                {
                    using (Pen p = new Pen(Settings.ModifiedColor.BackgroundColor, 1))
                    {
                        e.Graphics.DrawRectangle( p, new Rectangle( e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1 ) );
                    }
                }

                if( !Enabled )
                {
                    using( SolidBrush disabledRect = new SolidBrush( Color.FromArgb( 100, Color.Gray ) ) )
                    {
                        e.Graphics.FillRectangle( disabledRect, e.Bounds );
                    }
                }
            }

            private void GetElements()
            {
                Items.AddRange(Settings.ElementNames);
            }

            private void GetBrushes()
            {
                Settings.CombinedColor[] colors = Settings.ElementColors;

                int colorLength = colors.Length;
                elementFGBrushes = new SolidBrush[colorLength];
                elementBGBrushes = new SolidBrush[colorLength];

                for (int index = 0; index < colors.Length; index++)
                {
                    elementBGBrushes[index] = new SolidBrush(colors[index].BackgroundColor);
                    elementFGBrushes[index] = new SolidBrush(colors[index].ForegroundColor);
                }
            }

            protected override void OnKeyDown( KeyEventArgs e )
            {
                if (e.KeyData == Keys.F12)
                {
                    SetValuesAndDefaults(Defaults, Defaults);
                }
                base.OnKeyDown( e );
            }
        }
    }
}
