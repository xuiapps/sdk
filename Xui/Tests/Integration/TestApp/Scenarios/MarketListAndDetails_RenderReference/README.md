# Integration Test Run

## Timeline

# Market Pulse: a layer-composed 5,000-row grid

Fixed-seed fictional companies, compiled vector logos, 60-session sparklines and OHLC candles. One grid view composes struct layers for visible rows. No row views or recycler. Rows load on first visibility; full details load when opened. Allocation tests measure the managed canvas recording path for cached rows; native rasterization is outside that measurement.

### 01. MarketList

<img src="./01.MarketList.Render.svg" alt="01. MarketList expected" />

### 02. CompanyDetails

<img src="./02.CompanyDetails.Render.svg" alt="02. CompanyDetails expected" />

### 03. RowsUnderHeader

<img src="./03.RowsUnderHeader.Render.svg" alt="03. RowsUnderHeader expected" />
