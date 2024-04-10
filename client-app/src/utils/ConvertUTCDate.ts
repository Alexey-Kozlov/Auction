const ConvertUTCDate = (
  convertDate: Date | string | null | undefined
): Date | null => {
  let date: Date = new Date();

  if (convertDate) {
    if (convertDate instanceof String) {
      date = new Date(convertDate);
    } else if (convertDate instanceof Date) {
      date = convertDate;
    } else return null;
  }

  return new Date(
    Date.UTC(
      date.getUTCFullYear(),
      date.getUTCMonth(),
      date.getUTCDate(),
      date.getUTCHours(),
      date.getUTCMinutes(),
      date.getUTCSeconds()
    )
  );
};

export default ConvertUTCDate;
