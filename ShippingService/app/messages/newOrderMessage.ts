import {Order} from "./order";

export class NewOrderMessage {
    public static TypeID: string = 'QueuingMessages.NewOrderMessage:QueuingMessages';
    public TypeID: string = 'QueuingMessages.NewOrderMessage:QueuingMessages';

    constructor(public UserId: any, public Order: Order) {
    }
}