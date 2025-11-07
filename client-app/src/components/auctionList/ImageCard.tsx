import { useDispatch } from 'react-redux';
import { useGetImageForAuctionQuery } from '../../api/ImageApi';
import { useEffect } from 'react';
import { setCacheQuery } from '../../store/cacheSlice';
import { UrlCacheList } from '../../types';
import { PhotoProvider, PhotoView } from 'react-photo-view';
import { SlMagnifierAdd, SlMagnifierRemove } from 'react-icons/sl';
import { GrRotateLeft, GrRotateRight } from 'react-icons/gr';

const empty = require('../../assets/Empty.png');

type Props = {
  id?: string;
  dopStyle?: string;
  detail: boolean;
  cache: boolean;
};

export default function ImageCard({ id, dopStyle, detail, cache }: Props) {
  const imageQuery = useGetImageForAuctionQuery(
    { id: id ? id : '', cache: cache },
    {
      skip: !id,
    },
  );
  const dispatch = useDispatch();
  useEffect(() => {
    //запоминаем в кеше, что данная страница обработана и в кеше есть данные.
    //используем на странице списка аукционов, если было редактирование аукциона и в кеше
    //есть изображение - сбрасываем кеш
    if (id) {
      dispatch(
        setCacheQuery({
          urlImage: { cache: true, id: id },
        } as UrlCacheList),
      );
    }
    // eslint-disable-next-line
  }, [id]);

  return (
    <>
      {!imageQuery.isLoading &&
        !imageQuery.isFetching &&
        (detail ? (
          <PhotoProvider
            toolbarRender={({ onScale, scale, rotate, onRotate }) => {
              return (
                <>
                  <SlMagnifierAdd
                    className="PhotoView-Slider__toolbarIcon"
                    onClick={() => onScale(scale + 1)}
                    size={50}
                  />
                  <SlMagnifierRemove
                    className="PhotoView-Slider__toolbarIcon"
                    onClick={() => onScale(scale - 1)}
                    size={50}
                  />
                  <GrRotateLeft
                    className="PhotoView-Slider__toolbarIcon"
                    onClick={() => onRotate(rotate - 90)}
                    size={50}
                  />
                  <GrRotateRight
                    className="PhotoView-Slider__toolbarIcon"
                    onClick={() => onRotate(rotate + 90)}
                    size={50}
                  />
                </>
              );
            }}
          >
            <PhotoView
              key={1}
              src={
                imageQuery.data?.result?.image
                  ? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
                  : empty
              }
            >
              <img
                className="AuctionImageCardDetail"
                src={
                  imageQuery.data?.result?.image
                    ? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
                    : empty
                }
                alt=""
              />
            </PhotoView>
          </PhotoProvider>
        ) : (
          <img
            src={
              imageQuery.data?.result?.image
                ? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
                : empty
            }
            alt=""
            className={dopStyle ? dopStyle : 'AuctionImageCardList'}
          />
        ))}
    </>
  );
}
