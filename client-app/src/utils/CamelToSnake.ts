  export default function CamelToSnake(obj: any): any {
    if (typeof obj !== "object" || obj === null) {
      return obj;
    }

    if (Array.isArray(obj)) {
      return obj.map(CamelToSnake);
    }

    const newObj: any = {};
    for (const key in obj) {
      if (Object.prototype.hasOwnProperty.call(obj, key)) {
        const camelKey = key.replace(/([A-Z])/, (g) => g[0].toLowerCase());
        newObj[camelKey] = CamelToSnake(obj[key]); // Recursively convert nested objects/arrays
      }
    }
    return newObj;
  }