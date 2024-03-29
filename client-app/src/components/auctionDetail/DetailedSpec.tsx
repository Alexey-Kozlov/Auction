import { Table, Textarea } from "flowbite-react";
import { Auction } from "../../store/types";

type Props = {
    auction: Auction
}
export default function DetailedSpecs({ auction }: Props) {
    return (
        <Table striped={true}>
            <Table.Body className="divide-y">
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white">
                        Продавец
                    </Table.Cell>
                    <Table.Cell>
                        {auction?.seller}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white">
                        Наименование
                    </Table.Cell>
                    <Table.Cell>
                        {auction.title}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white">
                        Описание

                    </Table.Cell>
                    <Table.Cell>
                        {auction?.properties && auction.properties.split('\n').map((line, index) => {
                            return <p key={index}>{line}</p>;
                        })}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white">
                        Есть начальная цена?
                    </Table.Cell>
                    <Table.Cell>
                        {auction?.reservePrice > 0 ? `Да - ${auction?.reservePrice} руб.` : 'Нет'}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white">
                        Примечание
                    </Table.Cell>
                    <Table.Cell>
                        {auction?.description && auction.description.split('\n').map((line, index) => {
                            return <p key={index}>{line}</p>;
                        })}
                    </Table.Cell>
                </Table.Row>
            </Table.Body>
        </Table>
    );
}