using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.Data.Fields.FieldDomains;

namespace gView.Framework.Data.Fields.UI.FieldDomains
{
    public partial class Control_SimpleValueDomain : UserControl, IInitializeClass
    {
        private SimpleValuesDomain _domain;

        public Control_SimpleValueDomain()
        {
            InitializeComponent();
        }

        #region IInitializeClass Member

        public void Initialize(object parameter)
        {
            _domain = parameter as SimpleValuesDomain;

            if (_domain != null && _domain.Values != null)
            {
                foreach (object val in _domain.Values)
                {
                    if (val == null) continue;

                    if (!String.IsNullOrEmpty(txtValues.Text))
                        txtValues.Text += "\r\n";
                    txtValues.Text += val.ToString();
                }
            }
        }

        #endregion

        private void txtValues_TextChanged(object sender, EventArgs e)
        {
            if (_domain == null) return;

            string txt = txtValues.Text.Replace("\n", "");
            _domain.Values = txt.Split('\r');
        }
    }
}
