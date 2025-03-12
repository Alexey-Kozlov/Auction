import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import { useNavigate, useParams } from "react-router-dom";
import { Formik, Form, ErrorMessage } from "formik";
import TextInput from "../inputComponents/TextInput";
import * as Yup from "yup";
import {
	Auction,
	AuctionUpdated,
	Message,
	ProcessingState,
	RequestType,
} from "../../store/types";
import DatePickerInput from "../inputComponents/DatePickerInput";
import ImageFileInput from "../inputComponents/ImageFileInput";
import TextAreaInput from "../inputComponents/TextAreaInput";
import { Button } from "flowbite-react";
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
	const [image, setImage] = useState("");
	const [isWaiting, setIsWaiting] = useState(false);
	let auctionEndDate = new Date();
	auctionEndDate.setDate(auctionEndDate.getDate() + 1);
	const [newAuction, setNewAuction] = useState<Auction>({
		title: "",
		properties: "",
		auctionEnd: auctionEndDate,
		image: "",
		reservePrice: 0,
		description: "",
		error: "",
		usingImage: false,
	} as Auction);
	const auctionImage = useGetImageForAuctionQuery(
		{ id: newAuction.auctionId, cache: false },
		{ skip: newAuction.auctionId === undefined }
	);

	useEffect(() => {
		if (!auction.isLoading && auction.data) {
			setNewAuction((prev) => auction.data!.result);
		}
	}, [auction.data, auction.isLoading]);

	useEffect(() => {
		if (!auctionImage.isLoading && auctionImage.data?.result?.image) {
			setImage("data:image/png;base64, " + auctionImage?.data?.result?.image);
			setNewAuction((prev) => {
				return {
					...auction.data!.result,
					usingImage: auctionImage?.data?.result?.image ? true : false,
				};
			});
		}
	}, [auction.data, auctionImage.isLoading, auctionImage.data?.result?.image]);

	useEffect(() => {
		if (
			id !== "empty" &&
			!auction.isLoading &&
			auction.data?.isSuccess &&
			auction.data?.result &&
			!auction.data?.result.title
		) {
			navigate("/not-found");
		}
	}, [auction.isLoading, auction.data, id, navigate]);

	useEffect(() => {
		const eventStateAuctionUpdated = procState.find(
			(p) => p.eventName === "CollectionChanged" && p.ready
		);
		if (eventStateAuctionUpdated) {
			if (id && id !== "empty") {
				auctionImage.refetch();
				auction.refetch();
			}
			navigate("/");
		}
	}, [procState, auction, navigate, id, auctionImage]);

	useEffect(() => {
		if (id === "empty") {
			setCookie("RequestType", RequestType[RequestType.Create]);
		} else {
			setCookie("RequestType", RequestType[RequestType.Edit]);
		}
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

	const checkAuctionEndDate = (auctionEnd: Date): boolean => {
		//устанавливаем ограничение на ввод даты окончания аукциона - не меньше минуты от текущего времени
		let _date = new Date();
		_date = new Date(_date.getTime() + 30000);
		if (auctionEnd < _date) {
			const message: Message = {
				auctionId: "",
				message:
					"Дата окончания аукциона должна быть больше текущей даты не менее чем на 1 минуту",
				messageType: 0,
			};
			toast((p) => <ErrorMessageToast message={message} toastId={p.id} />);
			return false;
		}
		return true;
	};

	if (auction.isLoading) return "Загрузка...";

	return (
		<div className="mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg">
			<>
				<Heading
					title="Редактирование аукциона"
					subtitle="Отредактируйте данные ниже"
				/>
				<Formik
					initialValues={newAuction}
					enableReinitialize
					onSubmit={async (values) => {
						if (!checkAuctionEndDate(values.auctionEnd)) return;
						setIsWaiting(true);
						const auctionUpdated: AuctionUpdated = {
							auctionId: id,
							title: values.title,
							description: values.description ? values.description : "",
							properties: values.properties,
							auctionEnd: values.auctionEnd,
							reservePrice: values.reservePrice,
							image: values.image ? values.image : "",
							correlationId: uuid.v4() as string,
							usingImage: values.usingImage!,
						};
						dispatch(
							setEventFlag({ eventName: "CollectionChanged", ready: false })
						);
						if (id && id !== "empty") {
							//обновление аукциона
							await updateAuction(auctionUpdated);
						} else {
							//создание аукциона
							await createAuction(auctionUpdated);
						}
					}}
					validationSchema={Yup.object({
						title: Yup.string().required(
							"Необходимо указать наименование товара"
						),
						auctionEnd: Yup.date()
							.required("Необходимо указать дату и время окончания акциона")
							.min(
								new Date(),
								"Дата окончания аукциона должна быть больше текущей даты"
							),
					})}
				>
					{({
						handleSubmit,
						setFieldValue,
						isSubmitting,
						errors,
						isValid,
						dirty,
					}) => (
						<Form onSubmit={handleSubmit} autoComplete="off">
							<div className="mt-5">
								<TextInput
									name="title"
									placeholder="Наименование"
									label="Наименование"
									labellWidth="w-[250px]"
									inputWidth="w-[237px]"
									onChange={() => {}}
									required
								/>
							</div>
							<div className="mt-5">
								<TextAreaInput
									name="properties"
									label="Описание"
									placeholder="Описание"
									rows={5}
									labellWidth="w-[250px]"
									inputWidth="w-[400px]"
								/>
							</div>
							<div className="mt-5">
								<DatePickerInput
									name="auctionEnd"
									label="Дата окончания аукциона"
									labellWidth="w-[230px]"
									showTimeSelect
									showMonthDropdown
									showYearDropdown
									todayButton="Сегодня"
									closeOnScroll={true}
									timeCaption="time"
									locale="ru"
									dateFormat="dd.MM.yyyy HH:mm"
									timeIntervals={60}
									required
								/>
							</div>
							<div className="mt-5">
								<ImageFileInput
									name="image"
									label="Изображение"
									value={image}
									labellWidth="w-56"
									onChange={(imageData: string) => {
										setFieldValue("image", imageData);
										setImage(imageData);
									}}
									usingImage={(usingImg: boolean) => {
										setFieldValue("usingImage", usingImg);
									}}
								/>
							</div>

							{id === "empty" && (
								<div className="mt-5">
									<TextInput
										name="reservePrice"
										label="Начальная цена"
										type="number"
										placeholder="Начальная цена"
										labellWidth="w-[250px]"
										inputWidth="w-[237px]"
										onChange={() => {}}
									/>
								</div>
							)}
							<div className="mt-5">
								<TextAreaInput
									name="description"
									label="Примечание"
									placeholder="Примечание"
									rows={3}
									labellWidth="w-[250px]"
									inputWidth="w-[400px]"
								/>
							</div>
							<ErrorMessage name="error" render={() => <p>{errors.error}</p>} />
							<div className="flex justify-center m-5">
								<Button
									disabled={!isValid || !dirty || isSubmitting}
									isProcessing={isSubmitting || isWaiting}
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
						</Form>
					)}
				</Formik>
			</>
		</div>
	);
}
