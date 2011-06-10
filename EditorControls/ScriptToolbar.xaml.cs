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

namespace AxeSoftware.Quest.EditorControls
{
    public partial class ScriptToolbar : UserControl
    {
        public event Action Delete;
        public event Action MoveUp;
        public event Action MoveDown;
        public event Action Cut;
        public event Action Copy;
        public event Action Paste;
        public event Action PopOut;

        public ScriptToolbar()
        {
            InitializeComponent();
        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void cmdMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveUp();
        }

        private void cmdMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveDown();
        }

        private void cmdCut_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }

        private void cmdCopy_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        private void cmdPaste_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        private void cmdPopOut_Click(object sender, RoutedEventArgs e)
        {
            PopOut();
        }

        public bool CanCut
        {
            get { return cmdCut.IsEnabled; }
            set { cmdCut.IsEnabled = value; }
        }

        public bool CanCopy
        {
            get { return cmdCopy.IsEnabled; }
            set { cmdCopy.IsEnabled = value; }
        }

        public bool CanPaste
        {
            get { return cmdPaste.IsEnabled; }
            set { cmdPaste.IsEnabled = value; }
        }

        public bool CanDelete
        {
            get { return cmdDelete.IsEnabled; }
            set { cmdDelete.IsEnabled = value; }
        }

        public bool CanMoveUp
        {
            get { return cmdMoveUp.IsEnabled; }
            set { cmdMoveUp.IsEnabled = value; }
        }

        public bool CanMoveDown
        {
            get { return cmdMoveDown.IsEnabled; }
            set { cmdMoveDown.IsEnabled = value; }
        }

        public void HidePopOutButton()
        {
            popOutSeparator.Visibility = Visibility.Collapsed;
            cmdPopOut.Visibility = Visibility.Collapsed;
        }
    }
}