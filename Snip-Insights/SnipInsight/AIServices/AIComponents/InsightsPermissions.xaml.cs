// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows.Controls;
using SnipInsight.AIServices.AIViewModels;

namespace SnipInsight.AIServices.AIComponents
{
    /// <summary>
    /// Interaction logic for InsightsPermissions.xaml
    /// </summary>
    public partial class InsightsPermissions : UserControl
    {
        public InsightsPermissions()
        {
            InitializeComponent();
        }

        private void TextBlock_IsKeyboardFocusedChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
