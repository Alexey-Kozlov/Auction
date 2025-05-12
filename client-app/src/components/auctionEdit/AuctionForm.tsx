import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import { useNavigate, useParams } from "react-router-dom";
import TextInput from "../inputComponents/TextInput";
import {
	Auction,
	AuctionUpdated,
	FormErrors,
	ProcessingState,
	RequestType,
} from "../../types";
import DatePickerInput from "../inputComponents/DatePickerInput";
import ImageFileInput from "../inputComponents/ImageFileInput";
import TextAreaInput from "../inputComponents/TextAreaInput";
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
import toast from "react-hot-toast";
import ErrorMessageToast from "../signalRNotifications/ErrorMessageToast";
import { useCookies } from "react-cookie";
import { v4 as uuidv4 } from "uuid";
import {
	Button,
	Form,
	FormField,
	FormInput,
	FormTextArea,
	Grid,
	GridColumn,
	GridRow,
	Input,
	Label,
	Message,
	Segment,
	Table,
	TableBody,
	TableCell,
	TableRow,
	TextArea,
} from "semantic-ui-react";

export default function AuctionForm() {
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
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
		id: id,
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

	const auctionImage = useGetImageForAuctionQuery(
		{ id: newAuction.auctionId, cache: false },
		{ skip: newAuction.auctionId === undefined }
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
		const eventStateAuctionUpdated = procState.find(
			(p) => p.eventName === "CollectionChanged" && p.ready
		);
		if (eventStateAuctionUpdated) {
			if (id && id !== "empty") {
				auction.refetch();
			}
			navigate("/");
		}
		// eslint-disable-next-line
	}, [procState]);

	//первоначальная загрузка - устанавливаем параметры для логирования
	useEffect(() => {
		if (id === "empty") {
			setCookie("RequestType", RequestType[RequestType.Create]);
		} else {
			setCookie("RequestType", RequestType[RequestType.Edit]);
		}
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

	const handleTitleChanged = (value: string) => {
		setNewAuction((prev) => {
			return { ...prev, title: value };
		});
	};

	const handlePropertiesChanged = (value: string | number | undefined) => {
		setNewAuction((prev) => {
			return { ...prev, properties: value ? value.toString() : "" };
		});
	};

	const handleDescriptionChanged = (value: string | number | undefined) => {
		setNewAuction((prev) => {
			return { ...prev, description: value ? value.toString() : "" };
		});
	};

	const handleEndDateChanged = (value: Date) => {
		setNewAuction((prev) => {
			return { ...prev, auctionEnd: value };
		});
	};

	const handleImageChanged = (value: string) => {
		setNewAuction((prev) => {
			return { ...prev, image: value };
		});
	};

	const handleImageUsingChanged = (value: boolean) => {
		setNewAuction((prev) => {
			return { ...prev, usingImage: value };
		});
	};

	const handleReservePriceChanged = (value: number) => {
		setNewAuction((prev) => {
			return { ...prev, reservePrice: value };
		});
	};

	const [editError, setEditError] = useState<FormErrors | null>(null);

	const editErrorList: FormErrors[] = [
		{
			name: "EmptyTitle",
			topic: "Ошибка - пустое наименование аукциона!",
			detail: "Нужно указать наименование аукциона",
		},
		{
			name: "ErrorEndDate",
			topic: "Ошибка - дата окончания аукцилона!",
			detail: `Нужно указать дату окончания аукцилона не ранее чем за 1 день до текущей даты`,
		},
	];

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
	};

	if (auction.isLoading) return "Загрузка...";

	return (
		<Segment className="FormContainer">
			<Heading
				title="Редактирование аукциона"
				subtitle="Отредактируйте данные ниже"
			/>

			<Form onSubmit={handleSubmit} error={editError !== null}>
				<Table singleLine striped className="FormMainTable">
					<TableBody>
						<TableRow>
							<TableCell>
								Наименование<span>*</span>
							</TableCell>
							<TableCell>
								<FormInput
									placeholder="Наименование"
									onChange={(e, data) => handleTitleChanged(data.value)}
								/>
								<Message
									error
									visible={
										editErrorList.find((p) => p.name === "EmptyTitle") != null
									}
									header={editError?.topic}
									content={editError?.detail}
								/>
							</TableCell>
						</TableRow>
						<TableRow>
							<TableCell>Описание</TableCell>
							<TableCell>
								<FormTextArea
									placeholder="Описание"
									onChange={(e, data) => handlePropertiesChanged(data.value)}
								/>
							</TableCell>
						</TableRow>
						<TableRow>
							<TableCell>
								Дата окончания аукциона<span>*</span>
							</TableCell>
							<TableCell>
								<DatePickerInput
									showTimeSelect
									showMonthDropdown
									showYearDropdown
									setValue={newAuction.auctionEnd}
									getValue={(value) => handleEndDateChanged(value)}
								/>
								<Message
									visible={
										editErrorList.find((p) => p.name === "ErrorEndDate") != null
									}
									error
									header={editError?.topic}
									content={editError?.detail}
								/>
							</TableCell>
						</TableRow>
						<TableRow>
							<TableCell>Изображение</TableCell>
							<TableCell>
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
							</TableCell>
						</TableRow>
						<TableRow>
							<TableCell>Начальная цена</TableCell>
							<TableCell>
								<FormInput
									type="number"
									placeholder="Наименование"
									onChange={(e, data) =>
										handleReservePriceChanged(parseInt(data.value))
									}
								/>
							</TableCell>
						</TableRow>
						<TableRow>
							<TableCell>Примечание</TableCell>
							<TableCell>
								<FormTextArea
									placeholder="Примечание"
									onChange={(e, data) => handleDescriptionChanged(data.value)}
								/>
							</TableCell>
						</TableRow>
						<TableRow>
							<TableCell colSpan="2">
								<div className="flex justify-center mt-10">
									<Button
										// disabled={!isValid || !dirty || isSubmitting}
										// loading={isSubmitting || isWaiting}
										type="submit"
									>
										{id ? "Сохранить" : "Создать"}
									</Button>
									<Button
										className="ml-5"
										onClick={() => {
											navigate(-1);
										}}
									>
										Отмена
									</Button>
								</div>
							</TableCell>
						</TableRow>
					</TableBody>
				</Table>
			</Form>
		</Segment>
	);
}
