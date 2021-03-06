using Eto.Forms;
using Eto.GtkSharp.Forms.Cells;
using System;

namespace Eto.GtkSharp.Forms.Controls
{
	public interface IGridHandler
	{
		bool IsEventHandled(string handler);

		void ColumnClicked(GridColumnHandler column);
	}

	public interface IGridColumnHandler
	{
		GLib.Value GetValue(object dataItem, int dataColumn, int row);

		void BindCell(IGridHandler grid, ICellDataSource source, int columnIndex, ref int dataIndex);
	}

	public class GridColumnHandler : WidgetHandler<Gtk.TreeViewColumn, GridColumn>, GridColumn.IHandler, IGridColumnHandler
	{
		Cell dataCell;
		bool autoSize;
		bool editable;
		bool cellsAdded;
		IGridHandler grid;

		public GridColumnHandler()
		{
			Control = new Gtk.TreeViewColumn();
			AutoSize = true;
			Resizable = true;
			DataCell = new TextBoxCell();
		}

		public string HeaderText
		{
			get { return Control.Title; }
			set { Control.Title = value; }
		}

		public bool Resizable
		{
			get { return Control.Resizable; }
			set { Control.Resizable = value; }
		}

		public bool Sortable
		{
			get { return Control.Clickable; }
			set { Control.Clickable = value; }
		}

		public bool AutoSize
		{
			get
			{
				return autoSize;
			}
			set
			{
				autoSize = value;
				Control.Sizing = value ? Gtk.TreeViewColumnSizing.GrowOnly : Gtk.TreeViewColumnSizing.Fixed;
			}
		}

		void SetCellAttributes()
		{
			if (dataCell != null)
			{
				((ICellHandler)dataCell.Handler).SetEditable(Control, editable);
				SetupEvents();
			}
		}

		public bool Editable
		{
			get
			{
				return editable;
			}
			set
			{
				editable = value;
				SetCellAttributes();
			}
		}

		public int Width
		{
			get { return Control.Width; }
			set { Control.FixedWidth = value; }
		}

		public Cell DataCell
		{
			get
			{
				return dataCell;
			}
			set
			{
				dataCell = value;
			}
		}

		public bool Visible
		{
			get { return Control.Visible; }
			set { Control.Visible = value; }
		}

		public void BindCell(IGridHandler grid, ICellDataSource source, int columnIndex, ref int dataIndex)
		{
			this.grid = grid;
			if (dataCell != null)
			{
				var cellhandler = (ICellHandler)dataCell.Handler;
				if (!cellsAdded)
				{
					cellhandler.AddCells(Control);
					cellsAdded = true;
				}
				SetCellAttributes();
				cellhandler.BindCell(source, this, columnIndex, ref dataIndex);
			}
			SetupEvents();
		}

		public void SetupEvents()
		{
			if (grid == null)
				return;
			if (grid.IsEventHandled(Grid.CellEditingEvent))
				HandleEvent(Grid.CellEditingEvent);
			if (grid.IsEventHandled(Grid.CellEditedEvent))
				HandleEvent(Grid.CellEditedEvent);
			if (grid.IsEventHandled(Grid.ColumnHeaderClickEvent))
				HandleEvent(Grid.ColumnHeaderClickEvent);
			if (grid.IsEventHandled(Grid.CellFormattingEvent))
				HandleEvent(Grid.CellFormattingEvent);
		}

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				case Grid.ColumnHeaderClickEvent:
					var handler = new WeakReference(this);
					Control.Clicked += (sender, e) =>
					{
						var h = ((GridColumnHandler)handler.Target);
						if (h.grid != null)
							h.grid.ColumnClicked(h);
					};
					break;
				default:
					((ICellHandler)dataCell.Handler).HandleEvent(id);
					break;
			}
		}

		public GLib.Value GetValue(object dataItem, int dataColumn, int row)
		{
			if (dataCell != null)
				return ((ICellHandler)dataCell.Handler).GetValue(dataItem, dataColumn, row);
			return new GLib.Value((string)null);
		}
	}
}

