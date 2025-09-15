import { useEffect, useState } from "react";
import { Chart } from "primereact/chart";
import { DiagramTypes } from "./DiagramTypes";

type Props = {
  items: DiagramTypes[];
};

export default function Diagrams({ items }: Props) {
  // eslint-disable-next-line
  const [pieOptions, setPieOptions] = useState({
    plugins: {
      legend: {
        labels: {
          usePointStyle: true,
        },
      },
    },
  });
  const documentStyle = getComputedStyle(document.documentElement);
  const textColorSecondary = documentStyle.getPropertyValue(
    "--text-color-secondary"
  );
  const surfaceBorder = documentStyle.getPropertyValue("--surface-border");
  // eslint-disable-next-line
  const [barOptions, setBarOptions] = useState({
    maintainAspectRatio: false,
    aspectRatio: 0.7,
    scales: {
      x: {
        ticks: {
          color: textColorSecondary,
          font: {
            weight: 500,
          },
        },
        grid: {
          display: false,
          drawBorder: false,
        },
      },
      y: {
        ticks: {
          color: textColorSecondary,
        },
        grid: {
          color: surfaceBorder,
          drawBorder: false,
        },
      },
    },
  });
  const [chartData, setChartData] = useState({});

  const data = {
    labels: [...items.map((p) => p.Seller)],
    datasets: [
      {
        label: "Авторы аукционов",
        data: [...items.map((p) => p.ItemsCount)],
      },
    ],
  };
  useEffect(() => {
    setChartData(data);
    // eslint-disable-next-line
  }, []);
  return (
    <div className="CenterItem">
      <Chart
        type="pie"
        data={chartData}
        options={pieOptions}
        className="w-3"
      />
      <Chart
        type="bar"
        data={chartData}
        options={barOptions}
        className="w-3 ml-8"
      />
    </div>
  );
}
