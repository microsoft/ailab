
def find_by_property(self, property_name,expression):
    filtered_list = list(filter(lambda x: getattr(x,property_name) == expression , self))
    return next(iter(filtered_list),None)