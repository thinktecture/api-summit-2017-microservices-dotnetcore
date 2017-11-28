declare module server {
	interface newOrderMessage {
		order: {
			id: any;
			created: Date;
			items: any[];
		};
		userId: string;
	}
}
