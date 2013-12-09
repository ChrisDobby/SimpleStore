function BasketItem(quantity, product) {
    var item = this;

    item.quantity = quantity;
    item.product = product;

    item.cost = ko.computed(function () {
        return item.quantity * item.product.Price;
    });
}

function AppDataModel() {
    var self = this;

    self.basket = ko.observableArray([]);
}
