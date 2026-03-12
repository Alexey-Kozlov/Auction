import { useEffect, useState } from 'react';
import { useGetAuctionTagsQuery } from '../../api/TagApi';
import { Auction, State, TagCloudItem, TagList } from '../../types';
import { RendererFunction, Tag, TagCloud } from 'react-tagcloud';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { setParams } from '../../store/paramSlice';

type Props = {
  auction: Auction;
};

export default function Tags({ auction }: Props) {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const params = useSelector((state: RootState) => state.paramStore);

  const auctionTags = useGetAuctionTagsQuery(auction.itemId!, {
    skip: auction.itemId === '' || auction.itemId === undefined,
  });
  const [tagSelected, setTagSelected] = useState<TagCloudItem[]>([]);

  //получаем список тегов для текущего аукциона (запрос о тегах для данного аукциона)
  useEffect(() => {
    if (
      auctionTags &&
      !auctionTags.isFetching &&
      !auctionTags.isLoading &&
      auctionTags.data
    ) {
      setTagSelected(
        auctionTags.data?.result.map((item: TagList) => {
          return { value: item.tag, count: item.count } as TagCloudItem;
        }),
      );
    }
  }, [auctionTags]);

  const customRenderer: RendererFunction = (tag, size, color) => (
    <span
      key={tag.value}
      style={{
        fontSize: `${size / 2}em`,
        border: `2px solid ${color}`,
        borderColor: 'lightblue',
        borderRadius: '0.5rem',
        margin: '0.5rem',
        padding: '0.5rem',
        display: 'inline-block',
        color: 'blue',
        cursor: 'pointer',
      }}
    >
      {tag.value}
    </span>
  );

  return (
    <div>
      <TagCloud
        minSize={2}
        maxSize={5}
        tags={tagSelected}
        renderer={customRenderer}
        onClick={(tag: Tag) => {
          let urlParam: State = structuredClone(params);
          urlParam.tag = tag.value;
          dispatch(setParams(urlParam));
          return navigate('/');
        }}
      />
    </div>
  );
}
