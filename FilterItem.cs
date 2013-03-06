using System;
using System.Runtime.Serialization;

namespace Rankep.FilterableDataGridView
{
    /// <summary>
    /// Class representing a filter
    /// </summary>
    [Serializable()]
    public class FilterItem : ISerializable
    {
        /// <summary>
        /// The delegate behind the filter change event
        /// </summary>
        public delegate void FilterChangedHandler();

        /// <summary>
        /// Event for notification when the filter changes
        /// </summary>
        public event FilterChangedHandler FilterChanged;

        /// <summary>
        /// The filtering text
        /// </summary>
        private string _filter;
        /// <summary>
        /// Columns to filter
        /// </summary>
        private string _filterColumn;

        /// <summary>
        /// Gets or sets the filtering text
        /// </summary>
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                //Check subscribers
                if (FilterChanged != null)
                    FilterChanged(); //Fire event
            }
        }
        /// <summary>
        /// Gets or sets the filtered columns (Columns should be delimited with '|')
        /// </summary>
        public string FilterColumn
        {
            get
            {
                return _filterColumn;
            }
            set
            {
                _filterColumn = value;
                //Check subscribers
                if (FilterChanged != null)
                    FilterChanged(); //Fire event
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FilterItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter">Filtering text</param>
        /// <param name="column">Filtered columns</param>
        public FilterItem(string filter, string column)
        {
            _filter = filter;
            _filterColumn = column;
        }

        /// <summary>
        /// Constructor for deserialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public FilterItem(SerializationInfo info, StreamingContext context)
        {
            _filter = (string)info.GetValue("Filter", typeof(string));
            _filterColumn = (string)info.GetValue("FilterColumn", typeof(string));
        }

        /// <summary>
        /// Method supporting serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Filter", _filter);
            info.AddValue("FilterColumn", _filterColumn);
        }
    }

}
