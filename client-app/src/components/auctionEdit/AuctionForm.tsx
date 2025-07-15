import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import { useNavigate, useParams } from "react-router-dom";
import {
  Auction,
  AuctionUpdated,
  FormErrors,
  ProcessingState,
} from "../../types";

import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
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

export default function AuctionForm() {
  let { id } = useParams();
  if (!id) id = "empty";

  const auction = useGetDetailedViewDataQuery(id!, {
    skip: id === "empty",
  });
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore
  );
  const [createAuction] = useCreateAuctionMutation();
  const [updateAuction] = useUpdateAuctionMutation();

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

  const auctionImage = useGetImageForAuctionQuery(
    { id: newAuction.itemId, cache: false },
    { skip: newAuction.itemId === undefined }
  );

  //получаем данные по аукциону
  useEffect(() => {
    if (!auction.isLoading && auction.data && !auction.isFetching) {
      setNewAuction((prev) => auction.data!.result);
    }
    // eslint-disable-next-line
  }, [id, auction]);

  //получаем изображение аукциона
  useEffect(() => {
    if (
      auction.data &&
      !auctionImage.isLoading &&
      !auctionImage.isFetching &&
      auctionImage.data?.result?.image
    ) {
      setImage("data:image/png;base64, " + auctionImage?.data?.result?.image);
      setNewAuction((prev) => {
        return {
          ...auction.data!.result,
          usingImage: auctionImage?.data?.result?.image ? true : false,
        };
      });
    }
    // eslint-disable-next-line
  }, [id, auction, auctionImage]);

  //отлавливаем несуществующий адрес страницы
  useEffect(() => {
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

  //возврат на список аукционов после редактирования записи аукциона
  //при получении сообщения об изменении параметра CollectionChanged -
  //обновляем значения записи (удаляем кеширование), переходим на список аукционов
  useEffect(() => {
    if (
      procState.find(
        (p) => p.eventName === "CollectionChanged" && p.ready && isWaiting
      )
    ) {
      if (id && id !== "empty") {
        auction.refetch();
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
      itemId: id,
      title: newAuction.title,
      description: newAuction.description ? newAuction.description : "",
      properties: newAuction.properties,
      auctionEnd: newAuction.auctionEnd,
      reservePrice: newAuction.reservePrice,
      image: newAuction.image ? newAuction.image : "",
      correlationId: uuid.v4() as string,
      usingImage: newAuction.usingImage!,
    };
    dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
    dispatch(setEventFlag({ eventName: "CollectionChanged", ready: false }));
    if (id && id !== "empty") {
      //обновление аукциона
      await updateAuction(auctionUpdated);
    } else {
      //создание аукциона
      await createAuction(auctionUpdated);
    }
    //далее ждем сообщения о выполнении команды
  };

  if (auction.isLoading) return "Загрузка...";

  return (
    <div className="CenterItem">
      {procState.find((p) => p.eventName === "WaiterHide") &&
      !procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
        <Waiter />
      ) : (
        <></>
      )}
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
          <div className="col-3">Начальная цена</div>
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
                disabled={!isFormChanged || isWaiting}
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
