function CheckoutViewModel(app, dataModel) {
    var self = this;

    self.emailAddress = ko.observable("").extend({ required: true, email: true });
    self.confirmEmail = ko.observable("").extend({
        validation: {
            validator: function (val, params) {
                var otherValue = params;
                return val === ko.validation.utils.getValue(otherValue);
            },
            message: 'Email addresses do not match.',
            params: self.emailAddress
        }
    });
    self.name = ko.observable("").extend({ required : true});
    self.address1 = ko.observable("").extend({ required: true });
    self.address2 = ko.observable("");
    self.address3 = ko.observable("").extend({ required: true });
    self.postcode = ko.observable("").extend({ required: true });

    self.validationErrors = ko.validation.group([self.emailAddress, self.confirmEmail, self.name, self.address1, self.address2, self.address3, self.postcode]);

    self.basket = dataModel.basket;

    self.canCheckout = ko.computed(function() {
        return self.basket().length > 0;
    });

    self.removeFromBasket = function (item) {
        self.basket.remove(item);
    };

    self.totalGoods = ko.computed(function() {
        var total = 0;
        for (var i = 0; i < self.basket().length; i++) {
            total += self.basket()[i].cost();
        }
        return total;
    });

    self.totalPostage = ko.computed(function () {
        var postage = 0;
        for (var i = 0; i < self.basket().length; i++) {
            if (self.basket()[i].product.Postage > postage) {
                postage = self.basket()[i].product.Postage;
            }
        }
        return postage;
    });

    self.totalCost = ko.computed(function() {
        return self.totalGoods() + self.totalPostage();
    });

    self.checkoutAndPay = function (formElement) {
        if (self.validationErrors().length == 0) {
            return true;
        } 

        self.validationErrors.showAllMessages();
        return false;
    };
}

app.addViewModel({
    name: "Checkout",
    bindingMemberName: "checkout",
    factory: CheckoutViewModel
});
