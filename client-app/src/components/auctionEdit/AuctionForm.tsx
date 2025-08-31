import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import { useNavigate, useParams } from "react-router-dom";
import {
  Auction,
  AuctionUpdated,
  FormErrors,
  ProcessingState,
} from "../../types";

import api, { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
import { useDispatch, useSelector } from "react-redux";
import { setEventFlag } from "../../store/processingSlice";
import { RootState } from "../../store/store";
import {
  useCreateAuctionMutation,
  useUpdateAuctionMutation,
} from "../../api/ProcessingApi";
import uuid from "react-native-uuid";
import { InputText } from "primereact/inputtext";
import { InputTextarea } from "primereact/inputtextarea";
import DateInput from "../inputComponents/DateInput";
import { Panel } from "primereact/panel";
import ImageFileInput from "../inputComponents/ImageFileInput";
import {
  InputNumber,
  InputNumberValueChangeEvent,
} from "primereact/inputnumber";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import Waiter from "../Waiter";
import { CheckEventReady } from "../../utils/CheckEvent";
import { useSetUsersCurrentPageMutation } from "../../api/ServiceApi";

export default function AuctionForm() {
  let { id } = useParams();
  const auction = useGetDetailedViewDataQuery(id!, {
    skip: id === "",
  });

  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore
  );
  const [createAuction] = useCreateAuctionMutation();
  const [updateAuction] = useUpdateAuctionMutation();
  const [setCurrentPage] = useSetUsersCurrentPageMutation();

  const dispatch = useDispatch();
  const navigate = useNavigate();

  let auctionEndDate = new Date();
  //дата нового аукциона - на сутки вперед от текущей
  auctionEndDate.setDate(auctionEndDate.getDate() + 1);
  const [newAuction, setNewAuction] = useState<Auction>({
    itemId: id,
    title: "",
    properties: "",
    auctionEnd: auctionEndDate,
    image: "",
    reservePrice: 0,
    description: "",
    error: "",
    usingImage: false,
  } as Auction);
  const [image, setImage] = useState("");
  const [isWaiting, setIsWaiting] = useState(false);
  const [isFormChanged, setIsFormChanged] = useState(false);
  const [editError, setEditError] = useState<FormErrors | null>(null);

  const editErrorList: FormErrors[] = [
    {
      name: "EmptyTitle",
      message: "Нужно указать наименование аукциона",
    },
    {
      name: "ErrorEndDate",
      message: `Нужно указать дату окончания аукциона не ранее чем за 1 минуту до текущей даты`,
    },
  ];

  const fullImageQuery = useGetImageForAuctionQuery(
    { id: newAuction.itemId, cache: false },
    { skip: newAuction.itemId === undefined }
  );
  const cacheStore = useSelector((state: RootState) => state.cacheStore);
  const cachedImageQuery = useGetImageForAuctionQuery({
    id: cacheStore.urlImage.id,
    cache: cacheStore.urlImage.cache,
  });

  useEffect(() => {
    //получаем данные по аукциону
    if (!auction.isLoading && auction.data && !auction.isFetching) {
      setNewAuction((prev) => auction.data!.result);
      //посылаем вызов в апи процессинга - для записи в кеш редиса страницы, где находится пользователь
      setCurrentPage("/edit/");
    }
    //отлавливаем несуществующий адрес страницы
    if (
      id !== "empty" &&
      !auction.isLoading &&
      !auction.isFetching &&
      auction.data?.isSuccess &&
      auction.data?.result &&
      !auction.data?.result.title
    ) {
      navigate("/not-found");
    }
    // eslint-disable-next-line
  }, [id, auction]);

  //получаем изображение аукциона
  useEffect(() => {
    if (
      auction.data &&
      !fullImageQuery.isLoading &&
      !fullImageQuery.isFetching &&
      fullImageQuery.data?.result?.image
    ) {
      setImage("data:image/png;base64, " + fullImageQuery?.data?.result?.image);
      setNewAuction((prev) => {
        return {
          ...auction.data!.result,
          usingImage: fullImageQuery?.data?.result?.image ? true : false,
        };
      });
    }
    // eslint-disable-next-line
  }, [id, auction, fullImageQuery]);

  //возврат на список аукционов после редактирования записи аукциона
  //при получении сообщения об изменении параметра CollectionChanged -
  //удаляем кеширование и переходим на список аукционов
  useEffect(() => {
    if (!CheckEventReady(procState, "CollectionChanged") && isWaiting) {
      //ставим признак по обновлению описания аукциона
      auction.refetch();
      //ставим признак по обновлению списка аукционов
      //функцией api.util.resetApiState() - полностью удаляем кеш RTK для списка аукционрв,
      // иначе в пейджинге на других страницах останутся старые данные
      dispatch(api.util.resetApiState());
      //ставим признак по обновлению кешированного изображения
      if (!cachedImageQuery.isUninitialized) {
        cachedImageQuery.refetch();
      }
      //ставим признак по обновлению полного изображения
      if (!fullImageQuery.isUninitialized) {
        fullImageQuery.refetch();
      }
      navigate("/");
    }
    // eslint-disable-next-line
  }, [procState]);

  //хендлеры по изменению данных
  const handleTitleChanged = (value: string) => {
    setIsFormChanged(true);
    setEditError(() => null);
    setNewAuction((prev) => {
      return { ...prev, title: value };
    });
  };

  const handlePropertiesChanged = (value: string | number | undefined) => {
    setIsFormChanged(true);
    setEditError(() => null);
    setNewAuction((prev) => {
      return { ...prev, properties: value ? value.toString() : "" };
    });
  };

  const handleDescriptionChanged = (value: string | number | undefined) => {
    setIsFormChanged(true);
    setEditError(() => null);
    setNewAuction((prev) => {
      return { ...prev, description: value ? value.toString() : "" };
    });
  };

  const handleEndDateChanged = (value: Date) => {
    setIsFormChanged(true);
    setEditError(() => null);
    setNewAuction((prev) => {
      return { ...prev, auctionEnd: value };
    });
  };

  const handleImageChanged = (value: string) => {
    setIsFormChanged(true);
    setEditError(() => null);
    if (value) {
      handleImageUsingChanged(true);
    }
    setNewAuction((prev) => {
      return { ...prev, image: value };
    });
  };

  const handleImageUsingChanged = (value: boolean) => {
    setIsFormChanged(true);
    setEditError(() => null);
    setNewAuction((prev) => {
      return { ...prev, usingImage: value };
    });
  };

  const handleReservePriceChanged = (e: InputNumberValueChangeEvent) => {
    if (!e || !e.value || e.value < 0) return;
    setEditError(() => null);
    setIsFormChanged(true);
    setNewAuction((prev) => {
      return { ...prev, reservePrice: e.value } as Auction;
    });
  };

  const handleSubmit = async () => {
    //проверка на ошибки
    //время завершения
    let _date = new Date();
    _date = new Date(_date.getTime() + 60000);
    if (newAuction.auctionEnd < _date) {
      setEditError(() => editErrorList.find((p) => p.name === "ErrorEndDate")!);
      return;
    }
    //пустое наименование
    if (!newAuction.title) {
      setEditError(() => editErrorList.find((p) => p.name === "EmptyTitle")!);
      return;
    }
    //обработка данных
    setIsWaiting(() => true);
    const auctionUpdated: AuctionUpdated = {
      itemId: id!,
      title: newAuction.title,
      description: newAuction.description ? newAuction.description : "",
      properties: newAuction.properties,
      auctionEnd: newAuction.auctionEnd,
      reservePrice: newAuction.reservePrice,
      image: newAuction.image ? newAuction.image : "",
      correlationId: uuid.v4() as string,
      usingImage: newAuction.usingImage!,
    };
    // событие CollectionChanged для отслеживания значка ожидания, событие ожидания создается (ready=true)
    // после нажатия кнопки "Сохранить" - и снимается после получения сообщения в SignalRProvider
    // (ready=false)
    dispatch(setEventFlag({ eventName: "CollectionChanged", ready: true }));
    if (id && id !== "empty") {
      //обновление аукциона
      await updateAuction(auctionUpdated);
    } else {
      //создание аукциона
      await createAuction(auctionUpdated);
    }
    //далее ждем сообщения о выполнении команды
  };

  return (
    <div className="CenterItem">
      {CheckEventReady(procState, "CollectionChanged") ? <Waiter /> : <></>}
      <Panel className="EditForm">
        <Heading
          title="Редактирование аукциона"
          subtitle="Отредактируйте данные ниже"
        />

        <div className="grid mt-2 text-3xl">
          <div className="col-3">
            Наименование<span>*</span>
          </div>
          <div className="col-9">
            <InputText
              placeholder="Наименование"
              id="Title"
              className="InputControl w-full"
              value={newAuction.title}
              onChange={(e) => handleTitleChanged(e.target.value)}
            />
            <Message
              className="mt-2"
              severity="error"
              text={editError?.message}
              pt={{
                root: {
                  className:
                    editError !== null && editError.name === "EmptyTitle"
                      ? ""
                      : "hidden",
                },
              }}
            />
          </div>
          <div className="col-3">Описание</div>
          <div className="col-9">
            <InputTextarea
              id="Description"
              placeholder="Описание"
              rows={5}
              autoResize
              className="w-full text-2xl"
              value={newAuction.properties}
              onChange={(e) => handlePropertiesChanged(e.target.value)}
            />
          </div>
          <div className="col-3">
            Дата окончания аукциона<span>*</span>
          </div>
          <div className="col-9">
            <DateInput
              value={newAuction.auctionEnd}
              onChange={(e) => handleEndDateChanged(e!)}
              showOnFocus={true}
            />
            <Message
              className="mt-2"
              severity="error"
              text={editError?.message}
              pt={{
                root: {
                  className:
                    editError !== null && editError.name === "ErrorEndDate"
                      ? ""
                      : "hidden",
                },
              }}
            />
          </div>
          <div className="col-3">Изображение</div>
          <div className="col-9">
            <ImageFileInput
              name="image"
              value={image}
              onChange={(imageData: string) => {
                handleImageChanged(imageData);
                setImage(imageData);
              }}
              usingImage={(usingImg: boolean) => {
                handleImageUsingChanged(usingImg);
              }}
            />
          </div>
          {!id && <div className="col-3">Начальная цена</div>}
          {!id && (
            <div className="col-9">
              <InputNumber
                id="Title"
                placeholder="Начальная цена"
                className="InputControl w-full"
                value={newAuction.reservePrice}
                suffix=" руб"
                onValueChange={(e: InputNumberValueChangeEvent) =>
                  handleReservePriceChanged(e)
                }
              />
            </div>
          )}
          <div className="col-3">Примечание</div>
          <div className="col-9">
            <InputTextarea
              id="Description"
              rows={3}
              autoResize
              className="w-full"
              placeholder="Примечание"
              value={newAuction.description}
              onChange={(e) => handleDescriptionChanged(e.target.value)}
            />
          </div>
          <div className="col-12">
            <div className="CenterItem mt-10">
              <Button
                text
                raised
                rounded
                className="CustomButton w-16rem"
                disabled={
                  !isFormChanged ||
                  CheckEventReady(procState, "CollectionChanged")
                }
                onClick={(e) => {
                  e.preventDefault();
                  handleSubmit();
                }}
              >
                {id ? "Сохранить" : "Создать"}
              </Button>
              <Button
                text
                raised
                rounded
                className="CustomButton w-16rem ml-5"
                onClick={(e) => {
                  e.preventDefault();
                  navigate(-1);
                }}
              >
                Отмена
              </Button>
            </div>
          </div>
        </div>
      </Panel>
    </div>
  );
}
