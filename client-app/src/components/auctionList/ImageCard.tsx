import { useDispatch } from "react-redux";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
import { useEffect } from "react";
import { Image } from "primereact/image";
import { setCacheQuery } from "../../store/cacheSlice";
import { UrlCacheList } from "../../types";

const empty = require("../../assets/Empty.png");

type Props = {
  id?: string;
  dopStyle?: string;
  detail: boolean;
  cache: boolean;
};

export default function ImageCard({ id, dopStyle, detail, cache }: Props) {
  const imageQuery = useGetImageForAuctionQuery(
    { id: id ? id : "", cache: cache },
    {
      skip: !id,
    }
  );
  const dispatch = useDispatch();
  useEffect(() => {
    if (id) {
      dispatch(
        setCacheQuery({
          urlImage: { cache: true, id: id },
        } as UrlCacheList)
      );
    }
    // eslint-disable-next-line
  }, [id]);

  return (
    <>
      {!imageQuery.isLoading &&
        !imageQuery.isFetching &&
        (detail ? (
          <Image
            src={
              imageQuery.data?.result?.image
                ? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
                : empty
            }
            imageClassName="AuctionImageCardDetail"
            preview
            downloadable
          />
        ) : (
          <img
            src={
              imageQuery.data?.result?.image
                ? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
                : empty
            }
            alt=""
            className={dopStyle ? dopStyle : "AuctionImageCardList"}
          />
        ))}
    </>
  );
}
