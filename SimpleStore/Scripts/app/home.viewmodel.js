function HomeViewModel(app, dataModel) {
    var self = this;

    self.products = app.products;
    self.basket = dataModel.basket;

    self.productTotal = ko.computed(function () {
        var total = 0;
        for (var i = 0; i < self.basket().length; i++) {
            total += self.basket()[i].product.Price * self.basket()[i].quantity();
        }

        return total;
    });

    self.basketEmpty = ko.computed(function() {
        return self.basket().length == 0;
    });

    self.addToBasket = function (product) {
        var item;
        for (var i = 0; i < self.basket().length; i++) {
            if (self.basket()[i].product.Id == product.Id) {
                item = self.basket()[i];
                break;
            }
        }

        if (item == null) {
            self.basket.push(new BasketItem(1, product));
        } else {
            var q = item.quantity();
            q++;
            item.quantity(q);
        }
    };

    self.removeFromBasket = function (item) {
        self.basket.remove(item);
    };

    self.checkout = function () {
        location.hash = "checkout";
//        app.navigateToCheckout();
    };
}

app.addViewModel({
    name: "Home",
    bindingMemberName: "home",
    factory: HomeViewModel
});

