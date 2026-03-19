import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RendererFunction, Tag, TagCloud } from 'react-tagcloud';
import { useGetTagListQuery } from '../../api/TagApi';
import { State, TagCloudItem, TagList } from '../../types';
import { RootState } from '../../store/store';
import { setParams } from '../../store/paramSlice';

export default function TagFilter() {
  const dispatch = useDispatch();
  const params = useSelector((state: RootState) => state.paramStore);
  const tags = useGetTagListQuery({});
  const [tagSelected, setTagSelected] = useState<TagCloudItem[]>([]);

  useEffect(() => {
    if (
      tags &&
      !tags.isLoading &&
      !tags.isFetching &&
      !tags.isError &&
      tags.data &&
      tags.data.isSuccess
    ) {
      //ограничение на 5 тегов с максимальным количеством использования
      let orderTag: TagList[] = [];
      orderTag = tags.data?.result.slice().sort((a, b) => b.count - a.count);
      setTagSelected(
        orderTag.map((item: TagList) => {
          return { value: item.tag, count: item.count } as TagCloudItem;
        }),
      );
    }
    // eslint-disable-next-line
  }, [tags]);

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
        backgroundColor: `${tag.value === params.tag ? '#e0e0e1' : 'white'}`,
      }}
    >
      {tag.value}
    </span>
  );

  return (
    <div>
      <TagCloud
        minSize={2}
        maxSize={7}
        className="FilterTag"
        tags={tagSelected}
        renderer={customRenderer}
        shuffle={false}
        onClick={(tag: Tag) => {
          let urlParam: State = structuredClone(params);
          urlParam.tag = tag.value;
          urlParam.searchAdv = '';
          urlParam.searchTerm = '';
          dispatch(setParams(urlParam));
        }}
      />
    </div>
  );
}
