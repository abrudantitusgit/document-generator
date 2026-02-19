import React, { useState, useEffect, useMemo } from "react";
import FormTable from "../components/FormTable";
import moment from "moment";
import styled from "styled-components";
import {
  useTable,
  useFilters,
  useGlobalFilter,
  useAsyncDebounce,
  usePagination,
} from "react-table";
import * as FileSaver from "file-saver";
import * as XLSX from "xlsx";
import { baseUrl } from "../config/api";

const Styles = styled.div`
  table {
    border-spacing: 0;
    border: 1px solid black;
    table-layout: fixed;

    tr {
      :last-child {
        td {
          border-bottom: 0;
        }
      }
    }

    th,
    td {
      margin: 0;
      padding: 5px;
      text-align: center;
      :last-child {
        border-right: 0;
      }
    }

    tr:nth-child(even) {
      background-color: #dddddd;
    }

    td {
      padding: 0px;
      font-size: 15px;
    }
  }
`;

function GlobalFilter({ preGlobalFilteredRows, globalFilter, setGlobalFilter }) {
  const count = preGlobalFilteredRows.length;
  const [value, setValue] = React.useState(globalFilter);
  const onChange = useAsyncDebounce((val) => {
    setGlobalFilter(val || undefined);
  }, 200);

  return (
    <span>
      Search:{" "}
      <input
        value={value || ""}
        onChange={(e) => {
          setValue(e.target.value);
          onChange(e.target.value);
        }}
        placeholder={`${count} records...`}
        style={{
          fontSize: "1.1rem",
          border: "0",
        }}
      />
    </span>
  );
}

function DefaultColumnFilter({
  column: { filterValue, preFilteredRows, setFilter },
}) {
  const count = preFilteredRows.length;

  return (
    <input
      value={filterValue || ""}
      onChange={(e) => {
        setFilter(e.target.value || undefined);
      }}
      placeholder={`Search ${count} records...`}
    />
  );
}

function Table({ columns, data }) {
  const filterTypes = React.useMemo(
    () => ({
      text: (rows, id, filterValue) => {
        return rows.filter((row) => {
          const rowValue = row.values[id];
          return rowValue !== undefined
            ? String(rowValue)
                .toLowerCase()
                .startsWith(String(filterValue).toLowerCase())
            : true;
        });
      },
    }),
    []
  );

  const defaultColumn = React.useMemo(
    () => ({
      Filter: DefaultColumnFilter,
    }),
    []
  );

  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,

    // ✅ paginare
    page,
    canPreviousPage,
    canNextPage,
    pageOptions,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,

    prepareRow,
    state,
    visibleColumns,
    preGlobalFilteredRows,
    setGlobalFilter,
  } = useTable(
    {
      columns,
      data,
      defaultColumn,
      filterTypes,
      initialState: { pageIndex: 0, pageSize: 20 },
    },
    useFilters,
    useGlobalFilter,
    usePagination
  );

  const { pageIndex, pageSize } = state;

  return (
    <>
      <table {...getTableProps()}>
        <thead>
          {headerGroups.map((headerGroup) => (
            <tr {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column) => (
                <th {...column.getHeaderProps()}>
                  {column.render("Header")}
                  <div>{column.canFilter ? column.render("Filter") : null}</div>
                </th>
              ))}
            </tr>
          ))}

          <tr>
            <th colSpan={visibleColumns.length} style={{ textAlign: "left" }}>
              <GlobalFilter
                preGlobalFilteredRows={preGlobalFilteredRows}
                globalFilter={state.globalFilter}
                setGlobalFilter={setGlobalFilter}
              />
            </th>
          </tr>
        </thead>

        <tbody {...getTableBodyProps()}>
          {page.map((row) => {
            prepareRow(row);
            return (
              <tr {...row.getRowProps()}>
                {row.cells.map((cell) => (
                  <td {...cell.getCellProps()}>{cell.render("Cell")}</td>
                ))}
              </tr>
            );
          })}
        </tbody>
      </table>

      {/* ✅ controale paginare */}
      <div
        style={{
          display: "flex",
          gap: 8,
          alignItems: "center",
          marginTop: 12,
          flexWrap: "wrap",
        }}
      >
        <button onClick={() => gotoPage(0)} disabled={!canPreviousPage}>
          {"<<"}
        </button>
        <button onClick={() => previousPage()} disabled={!canPreviousPage}>
          {"<"}
        </button>
        <button onClick={() => nextPage()} disabled={!canNextPage}>
          {">"}
        </button>
        <button
          onClick={() => gotoPage(pageCount - 1)}
          disabled={!canNextPage}
        >
          {">>"}
        </button>

        <span style={{ marginLeft: 8 }}>
          Pagina{" "}
          <strong>
            {pageIndex + 1} / {pageOptions.length}
          </strong>
        </span>

        <span>
          | Mergi la pagina:{" "}
          <input
            type="number"
            min={1}
            max={pageOptions.length}
            defaultValue={pageIndex + 1}
            onChange={(e) => {
              const p = e.target.value ? Number(e.target.value) - 1 : 0;
              gotoPage(p);
            }}
            style={{ width: 80 }}
          />
        </span>

        <select
          value={pageSize}
          onChange={(e) => setPageSize(Number(e.target.value))}
        >
          {[10, 20, 50, 100].map((size) => (
            <option key={size} value={size}>
              Arată {size}
            </option>
          ))}
        </select>
      </div>

      <br />
    </>
  );
}

const RegistruATR = () => {
  const [registruATR, setRegistruATR] = useState([]);
  const [exportRegistruATR, setExportRegistruATR] = useState([]);

  const fileType =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8";
  const fileExtension = ".xlsx";

  const exportToCSV = (apiData, fileName) => {
    const ws = XLSX.utils.json_to_sheet(apiData);
    const wb = { Sheets: { Clienti: ws }, SheetNames: ["Clienti"] };
    const excelBuffer = XLSX.write(wb, { bookType: "xlsx", type: "array" });
    const data = new Blob([excelBuffer], { type: fileType });
    FileSaver.saveAs(data, fileName + fileExtension);
  };

  useEffect(() => {
    fetch(`${baseUrl}/proiect/proiecteinclude`)
      .then((response) => response.json())
      .then((data) => setRegistruATR(data))
      .catch((error) => console.log(error));
  }, []);

  useEffect(() => {
    fetch(`${baseUrl}/proiect/registru`)
      .then((response) => response.json())
      .then((data) => setExportRegistruATR(data))
      .catch((error) => console.log(error));
  }, []);

  const columns = useMemo(
    () => [
      {
        Header: "Client",
        columns: [
          {
            Header: "Nr. Proiect",
            accessor: "nrProiect",
          },
          {
            Header: "Client",
            accessor: (row) => {
              return row.client.nume;
            },
          },
          {
            Header: "Firma",
            accessor: (row) => {
              return row.firma ? row.firma.nume : "-";
            },
          },
        ],
      },
      {
        Header: "Proiect",
        columns: [
          {
            Header: "Nr. ATR",
            accessor: (row) => {
              return row.atr
                ? row.atr.nrAtr +
                    "/" +
                    moment(row.atr.dataAtr).format("DD.MM.yyyy")
                : " - / -";
            },
          },
          {
            Header: "Denumire",
            accessor: "denumire",
          },
          {
            Header: "Pret",
            accessor: (row) => {
              return row.atr ? row.atr.pret : "-";
            },
          },
          {
            Header: "Localitatea",
            accessor: "localitatea",
          },
          {
            Header: "Strada",
            accessor: "strada",
          },
          {
            Header: "Nr. Strada",
            accessor: "nrStrada",
          },
          {
            Header: "Apartament",
            accessor: "apartament",
          },
          {
            Header: "Data",
            accessor: (row) => {
              return row.data ? moment(row.data).format("DD.MM.yyyy") : "-";
            },
          },
          {
            Header: "Contract",
            accessor: (row) => {
              return row.contorContract ? row.contorContract.contor : "-";
            },
          },
          {
            Header: "Proces Verbal",
            accessor: (row) => {
              return row.contorProcesVerbal
                ? row.contorProcesVerbal.contor
                : "-";
            },
          },
          {
            Header: "Proces Verbal Cablu",
            accessor: (row) => {
              return row.contorProcesVerbal
                ? row.contorProcesVerbal.cablu
                : "-";
            },
          },
          {
            Header: "Buletin Cablu",
            accessor: (row) => {
              return row.contorBuletinCabluri.length > 0
                ? row.contorBuletinCabluri.map((item) => item.contor) + ","
                : "-";
            },
          },
          {
            Header: "Buletin Priza",
            accessor: (row) => {
              return row.contorBuletinPrize.length > 0
                ? row.contorBuletinPrize.map((item) => item.contor) + ","
                : "-";
            },
          },
        ],
      },
    ],
    []
  );

  const data = React.useMemo(() => registruATR, [registruATR]);

  return (
    <FormTable title="Registru ATR">
      <Styles>
        <button
          onClick={() =>
            exportToCSV(
              exportRegistruATR,
              "Registru Clienti - " + moment().format("DD.MM.yyyy")
            )
          }
        >
          Export Excel
        </button>

        <Table columns={columns} data={data} />
      </Styles>
    </FormTable>
  );
};

export default RegistruATR;
