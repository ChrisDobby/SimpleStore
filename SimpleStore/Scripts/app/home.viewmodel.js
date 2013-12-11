function OrderProduct(product, q) {
    var prod = this;

    prod.product = product;
    prod.quantity = ko.observable(q);
}

function HomeViewModel(app, dataModel) {
    var self = this;

    self.searchText = ko.observable("");

    self.products = ko.computed(function () {
        var prods = [];
        ko.utils.arrayForEach(app.products(), function (p) {
            if (p.Name.toLowerCase().indexOf(self.searchText()) >= 0) {
                prods.push(new OrderProduct(p, 1));
            }
        });

        return prods;
    });

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

    self.addToBasket = function (orderProduct) {
        if (orderProduct.quantity() <= 0) {
            return;
        }
        var item;
        for (var i = 0; i < self.basket().length; i++) {
            if (self.basket()[i].product.Id == orderProduct.product.Id) {
                item = self.basket()[i];
                break;
            }
        }

        if (item == null) {
            self.basket.push(new BasketItem(orderProduct.quantity(), orderProduct.product));
        } else {
            var q = item.quantity();
            q += parseInt(orderProduct.quantity());
            item.quantity(q);
        }
    };

    self.removeFromBasket = function (item) {
        self.basket.remove(item);
    };

    self.checkout = function () {
        location.hash = "checkout";
    };
}

app.addViewModel({
    name: "Home",
    bindingMemberName: "home",
    factory: HomeViewModel
});

