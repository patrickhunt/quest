﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("exits")]
    public partial class ExitsControl : UserControl, IElementEditorControl, IControlDataHelper
    {
        private ControlDataOptions m_options = new ControlDataOptions();
        private IEditorData m_data;
        private EditorController m_controller;
        private List<string> m_directionNames;
        private Dictionary<string, int> m_directionListIndexes = new Dictionary<string, int>();
        private bool m_selectionChanging;
        private const int k_outDirIndex = 4;

        public event EventHandler<DataModifiedEventArgs> Dirty { add { } remove { } }
        public event Action RequestParentElementEditorSave { add { } remove { } }

        public ExitsControl()
        {
            InitializeComponent();
            compassControl.HyperlinkClicked += compassControl_HyperlinkClicked;
            compassControl.SelectionChanged += compassControl_SelectionChanged;
            CompassEditor.EditExit += CompassEditor_EditExit;
            CompassEditor.CreateExit += CompassEditor_CreateExit;
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            m_controller = controller;
            m_controller.ElementsUpdated += m_controller_ElementsUpdated;

            m_directionNames = new List<string>(definition.GetListString("compass"));
        }

        void m_controller_ElementsUpdated()
        {
            Populate(m_data);
        }

        private class ExitListData
        {
            public string Name { get; set; }
            public IEditableObjectReference ToRoom { get; set; }
            public string Alias { get; set; }
            public string To
            {
                get { return ToRoom == null ? null : ToRoom.Reference; }
            }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;

            listView.Items.Clear();
            compassControl.Clear();
            m_directionListIndexes.Clear();
            PopulateCompassEditor(null);

            if (data != null)
            {
                IEnumerable<string> exits = m_controller.GetObjectNames("exit", data.Name);
                foreach (string exit in exits)
                {
                    IEditorData exitData = m_controller.GetEditorData(exit);

                    ExitListData exitListData = new ExitListData
                    {
                        Name = exitData.Name,
                        ToRoom = exitData.GetAttribute("to") as IEditableObjectReference,
                        Alias = exitData.GetAttribute("alias") as string,
                    };

                    int addedIndex = listView.Items.Add(exitListData);

                    if (m_directionNames.Contains(exitListData.Alias))
                    {
                        int direction = m_directionNames.IndexOf(exitListData.Alias);
                        compassControl.Populate(direction, exitListData.To);
                        m_directionListIndexes.Add(exitListData.Alias, addedIndex);
                    }
                }
            }
        }

        public void Save()
        {
        }

        public Control FocusableControl
        {
            get { return null; }
        }

        public Type ExpectedType
        {
            get { return null; }
        }

        public string Attribute
        {
            get { return null; }
        }

        public ControlDataOptions Options
        {
            get { return m_options; }
        }

        private WFToolbar toolbar
        {
            get { return (WFToolbar)toolbarHost.Child; }
        }

        private void toolbar_AddClicked()
        {
            m_controller.UIRequestAddElement("object", "exit", "");
        }

        private void toolbar_EditClicked()
        {
            if (listView.SelectedItem == null) return;
            m_controller.UIRequestEditElement(SelectedExit.Name);
        }

        private void toolbar_DeleteClicked()
        {
            string[] keys = listView.SelectedItems.Cast<ExitListData>().Select(i => i.Name).ToArray();
            if (keys.Length == 0) return;

            m_controller.StartTransaction(string.Format("Delete {0} exits", keys.Length));

            foreach (var key in keys)
            {
                m_controller.DeleteElement(key, false);
            }

            m_controller.EndTransaction();
        }

        private ExitListData SelectedExit
        {
            get
            {
                if (listView.SelectedItem == null) return null;
                return (ExitListData)listView.SelectedItem;
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toolbar.IsItemSelected = (listView.SelectedItems.Count > 0);
            if (m_selectionChanging) return;
            if (SelectedExit == null)
            {
                compassControl.SelectedDirection = null;
                PopulateCompassEditor(null);
            }
            else
            {
                int? dirIndex = m_directionNames.IndexOf(SelectedExit.Alias);
                if (dirIndex == -1) dirIndex = null;
                compassControl.SelectedDirection = dirIndex;
                PopulateCompassEditor(SelectedExit.Alias ?? string.Empty);
            }
        }

        private void compassControl_HyperlinkClicked(string destination)
        {
            m_controller.UIRequestEditElement(destination);
        }

        private void compassControl_SelectionChanged(int dirIndex)
        {
            string direction = m_directionNames[dirIndex];
            m_selectionChanging = true;
            listView.SelectedItems.Clear();
            if (m_directionListIndexes.ContainsKey(direction))
            {
                int listIndex = m_directionListIndexes[direction];
                listView.SelectedItem = listView.Items[listIndex];
            }
            m_selectionChanging = false;
            PopulateCompassEditor(direction);
        }

        private void PopulateCompassEditor(string direction)
        {
            // Populate the Compass Editor (top-right) in response to a selection change in
            // either the compass or exits list. There are four possibilities for direction:
            //   - an existing compass direction
            //   - a new compass direction
            //   - not a compass direction
            //   - null

            if (direction == null)
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.NoSelection;
                return;
            }
            else if (!m_directionNames.Contains(direction))
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.NotACompassExit;
                return;
            }
            else if (m_directionListIndexes.ContainsKey(direction))
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.ExistingCompassExit;
                ExitListData data = (ExitListData)listView.Items[m_directionListIndexes[direction]];
                CompassEditor.toName.Text = data.To;
                CompassEditor.ExitID = data.Name;
            }
            else
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.NewCompassExit;
                CompassEditor.to.Items.Clear();
                foreach (string objectName in m_controller.GetObjectNames("object"))
                {
                    CompassEditor.to.Items.Add(objectName);
                }
                CompassEditor.create.IsEnabled = false;
                CompassEditor.DirectionName = direction;
            }

            CompassEditor.direction.Text = AxeSoftware.Utility.Strings.CapFirst(direction);
            CompassEditor.chkCorresponding.IsChecked = DefaultCreateInverseSetting;

            // Allow creating the inverse exit, except for "out" as that has no inverse.
            CompassEditor.AllowCreateInverseExit = m_directionNames.IndexOf(direction) != k_outDirIndex;
        }

        private CompassEditorControl CompassEditor
        {
            get { return compassControl.compassEditor; }
        }

        private void CompassEditor_EditExit(string exitName)
        {
            m_controller.UIRequestEditElement(exitName);
        }

        private void CompassEditor_CreateExit(object sender, CompassEditorControl.CreateExitEventArgs e)
        {
            string newExit;
            if (CompassEditor.AllowCreateInverseExit) DefaultCreateInverseSetting = e.CreateInverse;
            if (e.CreateInverse)
            {
                newExit = m_controller.CreateNewExit(m_data.Name, e.To, e.Direction, GetInverseDirection(e.Direction));
            }
            else
            {
                newExit = m_controller.CreateNewExit(m_data.Name, e.To, e.Direction);
            }
            m_controller.UIRequestEditElement(newExit);
        }

        private static List<int> s_oppositeDirs = new List<int> { 8, 7, 6, 5, -1, 3, 2, 1, 0, 10, 9 };

        private string GetInverseDirection(string direction)
        {
            // 0 = NW, 1 = N, 2 = NE
            // 3 = W , 4 = O, 5 = E     9 = U, 10 = D
            // 6 = SW, 7 = S, 8 = SE

            // So opposites are:

            //  0 <--> 8
            //  1 <--> 7
            //  2 <--> 6
            //  3 <--> 5
            //  4 <--> not applicable
            //  5 <--> 3
            //  6 <--> 2
            //  7 <--> 1
            //  8 <--> 0
            //  9 <--> 10
            // 10 <--> 9

            int dirIndex = m_directionNames.IndexOf(direction);
            int opposite = s_oppositeDirs[dirIndex];
            return m_directionNames[opposite];
        }

        private const string k_regPath = @"Software\Quest\Settings";
        private const string k_regName = "DefaultCreateInverseSetting";

        private bool DefaultCreateInverseSetting
        {
            get
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(k_regPath);
                int? value =  key.GetValue(k_regName) as int?;
                return (value ?? 1) == 1;
            }
            set
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(k_regPath);
                key.SetValue(k_regName, value ? 1 : 0);
            }
        }
    }
}
