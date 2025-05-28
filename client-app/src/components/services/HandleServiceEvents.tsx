import { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { ProcessingState, RestoreDb, Session } from "../../types";
import {
	useElkIndexMutation,
	useSetSnapShotMutation,
	useRestoreSnapShotMutation,
	useResetImageCacheMutation,
} from "../../api/ServiceApi";

export default function HandleServiceEvents() {
	const dispatch = useDispatch();
	const events: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const sessionId = useSelector(
		(state: RootState) => state.paramStore
	).sessionId;
	const [elkIndex] = useElkIndexMutation();
	const [snapShotDb] = useSetSnapShotMutation();
	const [restoreSnapShot] = useRestoreSnapShotMutation();
	const [resetImageCache] = useResetImageCacheMutation();
	useEffect(() => {
		//запускаем переиндексацию
		if (
			events.find(
				(p) => p.eventName === "ElkIndex" && !p.ready && p.lastChanged
			)
		) {
			const sesion: Session = { sessionid: sessionId };
			elkIndex(sesion);
		}

		//запускаем создание снапшота
		if (
			events.find(
				(p) => p.eventName === "SetSnapShot" && !p.ready && p.lastChanged
			)
		) {
			const session: Session = { sessionid: sessionId };
			snapShotDb(session);
		}

		//запускаем восстановление снапшота
		if (
			events.find(
				(p) => p.eventName === "RestoreSnapShot" && !p.ready && p.lastChanged
			)
		) {
			let restoreData = events.find(
				(p) => p.eventName === "RestoreSnapShot" && !p.ready && p.lastChanged
			)?.param;
			const data: RestoreDb = {
				sessionid: sessionId,
				resetLog: restoreData.resetLog,
				restoreDate: restoreData.dateValue,
			};
			restoreSnapShot(data);
		}

		//запускаем сброс кеша изображений
		if (
			events.find(
				(p) => p.eventName === "ResetImageCache" && !p.ready && p.lastChanged
			)
		) {
			const session: Session = { sessionid: sessionId };
			resetImageCache(session);
		}
		// eslint-disable-next-line
	}, [events, sessionId]);

	return <></>;
}
