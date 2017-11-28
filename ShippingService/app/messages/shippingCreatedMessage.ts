export class ShippingCreatedMessage {
    public static TypeID: string = 'QueuingMessages.ShippingCreatedMessage:QueuingMessages';
    public TypeID: string = 'QueuingMessages.ShippingCreatedMessage:QueuingMessages';

    constructor(public Id: any, public Created: Date, public OrderId: any, public UserId: any) {
    }
}