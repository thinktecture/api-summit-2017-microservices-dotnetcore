declare module server {
	interface shippingCreatedMessage {
		id: any;
		created: Date;
		orderId: any;
		userId: string;
	}
}
