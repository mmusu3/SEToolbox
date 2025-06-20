namespace SEToolbox.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Microsoft.Xaml.Behaviors;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using Binding = System.Windows.Data.Binding;
    using DataObject = System.Windows.DataObject;
    using DragDropEffects = System.Windows.DragDropEffects;
    using ListBox = System.Windows.Controls.ListBox;
    using ListViewItem = System.Windows.Controls.ListViewItem;
    using MouseEventArgs = System.Windows.Input.MouseEventArgs;
    using MouseEventHandler = System.Windows.Input.MouseEventHandler;

    /// <summary>
    /// Multi Select Item Drag.
    /// </summary>
    public class ListBoxItemDragBehavior : Behavior<ListBoxItem>
    {
        #region fields

        private bool _isMouseClicked = false;
        private bool _wasDragging = false;
        private BindingBase _dragMemberBinding;

        #endregion

        #region Properties

        public BindingBase DragSourceBinding
        {
            get
            {
                return this._dragMemberBinding;
            }
            set
            {
                if (this._dragMemberBinding != value)
                {
                    this._dragMemberBinding = value;
                }
            }
        }

        #endregion

        #region methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseLeave += new MouseEventHandler(AssociatedObject_MouseLeave);
            this.AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        }

        #endregion

        #region events

        void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._isMouseClicked = true;

            var item = this.AssociatedObject.GetHitControl<ListViewItem>((MouseEventArgs)e);
            if (item != null && item.IsEnabled && item.IsSelected)
            {
                if ((NativeMethods.GetKeyState(Keys.ShiftKey) & KeyStates.Down) != KeyStates.Down
                    && (NativeMethods.GetKeyState(Keys.ControlKey) & KeyStates.Down) != KeyStates.Down)
                {
                    e.Handled = true;
                }
            }
        }

        void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = this.AssociatedObject.GetHitControl<ListViewItem>((MouseEventArgs)e);
            if (item != null && item.IsEnabled && item.IsSelected && this._isMouseClicked && !this._wasDragging)
            {
                if ((NativeMethods.GetKeyState(Keys.ShiftKey) & KeyStates.Down) != KeyStates.Down
                    && (NativeMethods.GetKeyState(Keys.ControlKey) & KeyStates.Down) != KeyStates.Down)
                {
                    var parent = ItemsControl.ItemsControlFromItemContainer(this.AssociatedObject) as ListBox;
                    parent.SelectedItems.Clear();
                    item.IsSelected = true;
                    item.Focus();
                }
            }
            this._isMouseClicked = false;
            this._wasDragging = false;
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this._isMouseClicked)
            {
                // set the item's DataContext as the data to be transferred.
                var dragObject = this.AssociatedObject.DataContext as IDragable;
                if (dragObject != null)
                {
                    this._wasDragging = true;
                    var data = new DataObject();

                    var parent = ItemsControl.ItemsControlFromItemContainer(this.AssociatedObject) as ListBox;
                    IList list = null;

                    if (this.DragSourceBinding == null)
                    {
                        // Pass the raw ItemSource as the drag object.
                        list = (IList)Activator.CreateInstance((typeof(List<>).MakeGenericType(dragObject.DataType)));
                        if (!this.AssociatedObject.IsSelected)
                        {
                            list.Add(this.AssociatedObject.DataContext);
                        }

                        foreach (var item in parent.SelectedItems)
                        {
                            list.Add(item);
                        }
                    }
                    else
                    {
                        // Pass the Binding object under the ItemSource as the drag object.
                        var propertyName = ((Binding)this.DragSourceBinding as Binding).Path.Path;
                        var pd = TypeDescriptor.GetProperties(this.AssociatedObject.DataContext).Find(propertyName, false);

                        list = (IList)Activator.CreateInstance((typeof(List<>).MakeGenericType(pd.PropertyType)));
                        if (!this.AssociatedObject.IsSelected)
                        {
                            list.Add(pd.GetValue(this.AssociatedObject.DataContext));
                        }

                        foreach (var item in parent.SelectedItems)
                        {
                            list.Add(pd.GetValue(item));
                        }
                    }

                    data.SetData(list.GetType(), list);

                    // Send the ListBox that initiated the drag, so we can determine if the drag and drop are different or not.
                    data.SetData(typeof(string), parent.Uid);

                    //data.SetData(dragObject.DataType, this.AssociatedObject.DataContext);
                    System.Windows.DragDrop.DoDragDrop(parent, data, DragDropEffects.Copy); // TODO Crash here on Safezone transfer
                    //System.Windows.DragDrop.DoDragDrop(this.AssociatedObject, data, DragDropEffects.Move);
                }
            }

            this._isMouseClicked = false;
        }
        #endregion
    }
}
