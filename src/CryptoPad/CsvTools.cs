using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;

namespace KsWare.CryptoPad {

	public static class CsvTools {

		public static void SaveAsCsv(this DataTable dtDataTable, string strFilePath, CsvOptions options) {
			using var streamWriter = new StreamWriter(strFilePath, false);
			ToCsv(dtDataTable, streamWriter, options);
		}

		public static void ToCsv(this DataTable dtDataTable, StreamWriter streamWriter, CsvOptions options) {
			var separator = options.Separator;

			// using var 
			//headers    
			for (var i = 0; i < dtDataTable.Columns.Count; i++) {
				streamWriter.Write(dtDataTable.Columns[i]);
				if (i < dtDataTable.Columns.Count - 1) {
					streamWriter.Write(options.Separator);
				}
			}

			streamWriter.Write(streamWriter.NewLine);
			foreach (DataRow dr in dtDataTable.Rows) {
				for (var i = 0; i < dtDataTable.Columns.Count; i++) {
					if (!Convert.IsDBNull(dr[i])) {
						var value = dr[i].ToString();
						if (value.Contains(separator)) {
							value = $"\"{value.Replace("\"", "\"\"")}\"";
							streamWriter.Write(value);
						}
						else {
							streamWriter.Write(dr[i].ToString());
						}
					}

					if (i < dtDataTable.Columns.Count - 1) {
						streamWriter.Write(separator);
					}
				}

				streamWriter.Write(streamWriter.NewLine);
			}

			streamWriter.Flush();
		}

		//csv to DataTable
		public static DataTable GetDataTable(TextFieldParser parser) {
			var csvData = new DataTable();

			parser.HasFieldsEnclosedInQuotes = true;
			var colFields = parser.ReadFields();
			foreach (var column in colFields) {
				var dataColumn = new DataColumn(column);
				dataColumn.AllowDBNull = true;
				csvData.Columns.Add(dataColumn);
			}

			while (!parser.EndOfData) {
				var fieldData = parser.ReadFields();
				//Making empty value as null
				for (var i = 0; i < fieldData.Length; i++) {
					if (fieldData[i] == "") {
						fieldData[i] = null;
					}
				}

				csvData.Rows.Add(fieldData);
			}

			return csvData;
		}
	}

	public class CsvOptions {
		public char Separator { get; set; } = ',';
		public char Quotes { get; set; } = '\"';
	}

	public class CsvProperties {
		public ColumnProperties[] Columns { get; set; }

		public void StoreColumnWidth(DataGrid dataGrid) {
			Columns = dataGrid.Columns.Select(col => new ColumnProperties(col)).ToArray();
		}
	}

	public class ColumnProperties {

		public ColumnProperties(DataGridColumn col) {
			Save(col);
		}

		public void Save(DataGridColumn col) {
			Width = col.ActualWidth;
			// col.IsFrozen;
			// col.IsReadOnly;
			// col.MinWidth;
			// col.MaxWidth;
			// col.SortDirection;
			// col.Visibility;
			// col.CanUserReorder;
			// col.CanUserResize;
			// col.CanUserSort;
			// col.DisplayIndex;
		}

		public void Load(DataGridColumn col) {
			if(Width.HasValue) col.Width = Width.Value;
		}

		public double? Width { get; set; }
	}

}
