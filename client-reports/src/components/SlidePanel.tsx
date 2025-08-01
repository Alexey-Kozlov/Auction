import React, { useEffect, useState } from "react";
import { ParameterItem, ParameterSelect, ParameterType } from "../types";
import { useDispatch, useSelector } from "react-redux";
import { setParamIsOpen, setReportLoading } from "../store/ReportSlice";
import { RootState } from "../store/Store";
import { Button } from "primereact/button";
import { Sidebar } from "primereact/sidebar";
import { InputText } from "primereact/inputtext";
import { InputNumber } from "primereact/inputnumber";
import { Dropdown } from "primereact/dropdown";

type Props = {
  params: ParameterItem[];
  reportName: string;
};

export default function SlidePanel({ params, reportName }: Props) {
  const [paramValue, setParamValue] = useState<string[]>([]);
  const dispatch = useDispatch();
  const reportStore = useSelector((state: RootState) => state.reportStore);
  const [showPanel, setShowPanel] = useState(true);

  useEffect(() => {
    let paramVal: string[] = [];
    params.forEach((item, index) => {
      paramVal[index] = item.Value;
    });
    setParamValue(paramVal);
    document.getElementById("rt")?.focus();
    // eslint-disable-next-line
  }, []);

  const convertSelectItem = (ind: any) => {
    //создаем option объект без свойства Default - иначе не работает контрол DropDown
    let item = JSON.parse(paramValue[ind]).find(
      (p: any) => p.Default === true
    ) as ParameterSelect;
    return { Label: item.Label, Value: item.Value };
  };

  const GetParamControl = (item: ParameterItem, index: number) => {
    if (item.Type === ParameterType.Select) {
      //создаем option объект без свойства Default - иначе не работает контрол DropDown
      var selectOptions: any = [];
      (JSON.parse(item.Value) as ParameterSelect[]).forEach((item) => {
        selectOptions.push({ Label: item.Label, Value: item.Value });
      });
      return (
        <Dropdown
          options={selectOptions}
          name={item.Id}
          optionLabel="Label"
          value={convertSelectItem(index)}
          onChange={(p) => {
            if (!p.target.value) return;
            setParamValue((st) => {
              let _item: ParameterSelect[] = JSON.parse(st[index]);
              _item.forEach((st) => {
                if (st.Default) st.Default = false;
                if (st.Value === p.value.Value) st.Default = true;
              });
              st[index] = JSON.stringify(_item);
              return [...st];
            });
          }}
        />
      );
    }
    if (item.Type === ParameterType.Bool) {
      return (
        <input
          type="checkbox"
          name={item.Id}
          checked={paramValue![index].toLowerCase() === "true"}
          onChange={(p) => {
            if (!p.target.value) return;
            setParamValue((st) => {
              st[index] = p.target.checked ? "true" : "false";
              return [...st];
            });
          }}
        />
      );
    }
    if (item.Type === ParameterType.Text) {
      return (
        <InputText
          name={item.Id}
          className="InputControl"
          value={paramValue![index]}
          onChange={(p) => {
            setParamValue((st) => {
              st[index] = p.target.value;
              return [...st];
            });
          }}
        />
      );
    }
    if (item.Type === ParameterType.Number) {
      return (
        <InputNumber
          name={item.Id}
          className="InputControl"
          value={parseInt(paramValue![index])}
          onChange={(p) => {
            setParamValue((st) => {
              st[index] = p.value!.toString();
              return [...st];
            });
          }}
        />
      );
    }
  };

  const SetParams = (paramList: string[]): ParameterItem[] => {
    //создаем параметр запроса используя глубокое клонирование
    let tmp = structuredClone(params);
    paramList.forEach((item, index) => {
      try {
        //извлекаем выбранное значение (если было перечисление значений)
        tmp[index].Value = JSON.parse(paramList[index]).find(
          (p: ParameterSelect) => p.Default
        ).Value;
      } catch {
        //здесь если не было выбора из перечисления - просто текстовое поле или булево значение
        tmp[index].Value = paramList[index];
      }
    });
    return tmp;
  };

  const reportSubmit = async (
    e:
      | React.MouseEvent<HTMLButtonElement, MouseEvent>
      | React.KeyboardEvent<HTMLDivElement>
  ) => {
    e.preventDefault();
    dispatch(setReportLoading({ param: SetParams(paramValue) }));
    setShowPanel(false);
  };

  const hitEnter = (e: React.KeyboardEvent<HTMLDivElement>) => {
    if (e.key === "Enter") reportSubmit(e);
  };

  return (
    <div onKeyDown={(e) => hitEnter(e)}>
      <div
        className="ParameterButton"
        onClick={() => setShowPanel(true)}
      >
        Параметры отчета
      </div>
      <Sidebar
        visible={showPanel}
        position="right"
        onHide={() => setShowPanel(false)}
      >
        <div>
          <input
            type="checkbox"
            id="nav-toggle"
            hidden
            checked={reportStore.paramIsOpen}
            readOnly
          ></input>
          <nav className="nav">
            <div className="text-center text-4xl">Параметры отчета</div>
            <div className="text-center text-4xl mb-4">"{reportName}"</div>
            <form>
              <div
                className="ParameterContainer"
                onClick={(e) => e.stopPropagation()}
              >
                {paramValue &&
                  paramValue!.length !== 0 &&
                  params.map((p, index) => {
                    return (
                      <React.Fragment key={index}>
                        <div className="grid w-full">
                          <div className="col-5 ParameterItem">{p.Label}</div>
                          <div className="col-7 ParameterItem">
                            {GetParamControl(p, index)}
                          </div>
                        </div>
                      </React.Fragment>
                    );
                  })}
              </div>
              <div className="CenterItem mt-4">
                <Button
                  text
                  raised
                  rounded
                  className="CustomButton mr-4 w-20rem"
                  onClick={(e) => reportSubmit(e)}
                >
                  Получить отчет
                </Button>
              </div>
            </form>
          </nav>
        </div>
      </Sidebar>
    </div>
  );
}
