using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using HabboGallery.Desktop.Helpers;

namespace HabboGallery.Desktop.Controls
{
    public class NotifiableForm : Form, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Binding> _bindings;

        protected Program Master => Program.Master;

        public NotifiableForm()
        {
            _bindings = new Dictionary<string, Binding>();

            StartPosition = FormStartPosition.CenterScreen;
        }

        protected void Bind(IBindableComponent component, string propertyName, string dataMember, IValueConverter converter = null, DataSourceUpdateMode dataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged)
        {
            var binding = new CustomBinding(propertyName, this, dataMember, converter)
            {
                DataSourceUpdateMode = dataSourceUpdateMode,
                ControlUpdateMode = ControlUpdateMode.OnPropertyChanged
            };
            component.DataBindings.Add(binding);
            _bindings[dataMember] = binding;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Owner != null)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2, Owner.Location.Y + Owner.Height / 2 - Height / 2);
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                FindForm()?.Invoke(handler, this, e);
            }
            if (DesignMode)
            {
                _bindings[e.PropertyName].ReadValue();
            }
        }
        protected void RaiseOnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
