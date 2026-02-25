export const checkEventReady = (
  eventCollection: any,
  name: string,
): boolean => {
  const rezult =
    eventCollection.find((p: any) => p.eventName === name) &&
    eventCollection.find((p: any) => p.eventName === name && p.ready);
  return rezult ? true : false;
};

export const checkEventNotReady = (
  eventCollection: any,
  name: string,
): boolean => {
  const rezult =
    eventCollection.find((p: any) => p.eventName === name) &&
    eventCollection.find((p: any) => p.eventName === name && !p.ready);
  return rezult ? true : false;
};

export const checkEventLastChangedNotReady = (
  eventCollection: any,
  name: string,
): boolean => {
  const rezult =
    eventCollection.find((p: any) => p.eventName === name) &&
    eventCollection.find(
      (p: any) => p.eventName === name && !p.ready && p.lastChanged,
    );
  return rezult ? true : false;
};

export const checkEventLastChangedReady = (
  eventCollection: any,
  name: string,
): boolean => {
  const rezult =
    eventCollection.find((p: any) => p.eventName === name) &&
    eventCollection.find(
      (p: any) => p.eventName === name && p.ready && p.lastChanged,
    );
  return rezult ? true : false;
};
