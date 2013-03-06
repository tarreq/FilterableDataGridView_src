using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Linq;

/* 
 * Many thanks to Kabwla.Phone from codeproject.com for suggestions to make the control better.
 */

namespace Rankep.FilterableDataGridView
{
    /// <summary>
    /// The class extends the functionality of a DataGridView with filtering
    /// </summary>
    [Serializable()]
    [ToolboxBitmap(typeof(DataGridView))]
    public partial class FilterableDataGridView : DataGridView
    {
        private object syncroot = new object();

        /// <summary>
        /// Indicates whether the filters are in updating mode.
        /// Call BeginFilterUpdate() to enter updating mode, and EndFilterUpdate() to exit.
        /// </summary>
        public bool IsFilterUpdating { get; private set; }

        /// <summary>
        /// Indicates whether the content should be filtered after updating mode is exited.
        /// </summary>
        public bool IsFilterDirty { get; private set; }

        /// <summary>
        /// A collection to store the filters
        /// </summary>
        /// <remarks>
        /// ObservableCollection requires .NET 4.0
        /// </remarks>
        private ObservableCollection<FilterItem> _filters;

        /// <summary>
        /// Property to get or set the collection of the filters
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObservableCollection<FilterItem> Filters
        {
            get
            {
                return _filters;
            }
            set
            {
                //Remove eventhandler so the original collection is 'free' and can be garbage collected.
                if (_filters != null)
                {
                    _filters.CollectionChanged -= _filters_CollectionChanged;
                }

                _filters = value;

                if (_filters != null)
                {
                    //remove again, it shold not be linked, however to be sure. 
                    _filters.CollectionChanged -= _filters_CollectionChanged;

                    // Subscribe to the CollectionChanged event, so the filtering can be done automatically
                    _filters.CollectionChanged += _filters_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// The event handler of the filters CollectionChanged event, 
        /// so the filtering can be automatic when the collection changes
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Event arguments</param>
        void _filters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Indicates whether the collection really changed
            bool changed = true;

            //If a new element is added
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (FilterItem fi in e.NewItems)
                {
                    //If the collection already contains this FilterItem
                    if (_filters.Count(x => x.Equals(fi)) > 1)
                    {
                        lock (syncroot)
                        {
                            //Disable the eventhandler while removing the item
                            _filters.CollectionChanged -= _filters_CollectionChanged;
                            //Remove the newly added FilterItem from the collection
                            _filters.RemoveAt(e.NewItems.IndexOf(fi));
                            //Enable the eventhandler
                            _filters.CollectionChanged += _filters_CollectionChanged;
                            //The collection actually didn't change, there's no need to refilter
                            changed = false;
                        }
                    }
                    else
                    {
                        //subscribe to its event, so filtering will be done automatically every time when the filter changes
                        fi.FilterChanged += Filter;
                    }
                }
            }
            //If the filter is removed
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (FilterItem fi in e.OldItems)
                {
                    //unsubscribe from its event
                    fi.FilterChanged -= Filter;
                }
            }

            //Finally filter the list
            if (changed)
                Filter();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FilterableDataGridView()
        {
            InitializeComponent();

            Filters = new ObservableCollection<FilterItem>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container"></param>
        public FilterableDataGridView(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Filters = new ObservableCollection<FilterItem>();
        }

        /// <summary>
        /// Sets the IsFilterUpdating property to true, 
        /// so you can perform multiple changes 
        /// and the filtering will be done only after EndFilterUpdate() is called
        /// </summary>
        public void BeginFilterUpdate()
        {
            IsFilterUpdating = true;
        }

        /// <summary>
        /// Sets the IsFilterUpdating property to false,
        /// and performs the filtering if it is necessary
        /// </summary>
        public void EndFilterUpdate()
        {
            IsFilterUpdating = false;
            if (IsFilterDirty)
                Filter();
        }

        /// <summary>
        /// Method to filter the DataGridView
        /// It gets called whenever the filters collection changes or a filter changes.
        /// Explicit call is possible, however not necessary.
        /// </summary>
        public void Filter()
        {
            if (IsFilterUpdating)
            {
                IsFilterDirty = true;
            }
            else
            {
                IsFilterDirty = false;
                //Set the selected cell to null, so every cell(/row) can be hidden
                this.CurrentCell = null;
                //Check every row in the DataGridView
                foreach (DataGridViewRow row in Rows)
                {
                    //Skip the NewRow
                    if (row.Index != this.NewRowIndex)
                    {
                        bool visible = true;
                        //Check every FilterItem
                        foreach (FilterItem fi in _filters)
                        {
                            //The char used to delimit the columns from each other
                            // it is also used to denote an OR relation ship between the filter texts
                            char c = '|';
                            //Split the string to column names
                            List<string> columns = new List<string>(fi.FilterColumn.Split(c));
                            List<string> filters = new List<string>(fi.Filter.Split(c));
                            bool atLeastOneContains = false;

                            //Check every columns
                            foreach (string column in columns)
                            {
                                foreach (string filter in filters)
                                {
                                    if (row.Cells[column].Value != null && row.Cells[column].Value.ToString().ToUpper().Contains(filter.ToUpper()))
                                    {
                                        //If the column contains any of the filter texts, the filter is satisfied
                                        atLeastOneContains = true;
                                        break;
                                    }
                                }
                                if (atLeastOneContains)
                                {
                                    //If the column contains the filter text, the filter is satisfied
                                    break;
                                }
                            }
                            //If none of the columns contain the text, the row can't be visible
                            if (!atLeastOneContains)
                            {
                                visible = false;
                                break;
                            }
                        }
                        //Set the Visible property the Row
                        row.Visible = visible;
                    }
                }
            }
        }
    }
}
