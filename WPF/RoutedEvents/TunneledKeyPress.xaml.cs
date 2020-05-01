﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RoutedEvents
{
    /// <summary>
    /// TunneledKeyPress.xaml 的交互逻辑
    /// </summary>
    public partial class TunneledKeyPress : Window
    {
        public TunneledKeyPress()
        {
            InitializeComponent();
        }

        protected int eventCounter = 0;

        private void SomeKeyPressed(object sender, KeyEventArgs e)
        {
            eventCounter++;
            string message = "#" + eventCounter.ToString() + ";\r\n" +
                "Sender=> " + sender.ToString() + "\r\n" +
                "Source=> " + e.Source + "\r\n" +
                "Original Source=> " + e.OriginalSource + "\r\n" +
                "Event: " + e.RoutedEvent;
            LstMessages.Items.Add(message);
            e.Handled = (bool) ChkHandle.IsChecked;
        }

        private void CmdClear_Click(object sender, RoutedEventArgs e)
        {
            eventCounter = 0;
            LstMessages.Items.Clear();
        }
    }
}