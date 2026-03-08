export const CheckEventReady = (
  eventCollection: any,
  name: string,
): boolean => {
  const rezult =
    eventCollection.find((p: any) => p.eventName === name) &&
    eventCollection.find((p: any) => p.eventName === name && p.ready);
  return rezult ? true : false;
};

export const CheckEventNotReady = (
  eventCollection: any,
  name: string,
): boolean => {
  const rezult =
    eventCollection.find((p: any) => p.eventName === name) &&
    eventCollection.find((p: any) => p.eventName === name && !p.ready);
  return rezult ? true : false;
};

export const CheckEventLastChangedNotReady = (
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

export const CheckEventLastChangedReady = (
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
